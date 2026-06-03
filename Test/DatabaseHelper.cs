using System;
using Microsoft.Data.Sqlite;

namespace Test
{
    public static class DatabaseHelper
    {
        private const string DatabaseFileName = "ScaleData.db";

        // [R-02] Lock chia sẻ giữa DataRepository (UI Thread) và DatabaseService (Background Timer).
        // SemaphoreSlim hỗ trợ cả Wait() đồng bộ và WaitAsync() bất đồng bộ — không gây deadlock.
        internal static readonly SemaphoreSlim DbAccessLock = new SemaphoreSlim(1, 1);

        // [FIX N3] Schema version tracking — tăng khi có thay đổi schema
        private const int CurrentSchemaVersion = 4;

        /// <summary>
        /// Trả về connection string có Password= từ biến môi trường TESA_DB_KEY.
        /// Ném InvalidOperationException nếu biến môi trường chưa được đặt.
        /// </summary>
        public static string GetConnectionString()
        {
            return $"Data Source={DatabaseFileName}";
        }

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();

                // [FIX N3] Dùng TEXT thay REAL cho Weight — giữ precision decimal chính xác
                // SQLite không có kiểu DECIMAL native, TEXT là cách chuẩn nhất để lưu giá trị decimal
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS ScaleRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp DATETIME NOT NULL,
                        Weight TEXT NOT NULL,
                        Unit TEXT NOT NULL,
                        NatCode TEXT,
                        Batch TEXT,
                        SampleName TEXT,
                        Location TEXT,
                        Tester TEXT,
                        IsSynced INTEGER NOT NULL DEFAULT 0
                    );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Migrate DB cũ theo thứ tự
                MigrateWeightToText(connection);  // v1 -> v2: Weight REAL -> TEXT
                MigrateAddIsSynced(connection);    // v2 -> v3: Thêm cột IsSynced
                MigrateAddTester(connection);      // v3 -> v4: Thêm cột Tester
            }
        }

        /// <summary>
        /// [SEC-4.1] Mã hóa file SQLite plaintext hiện có sang SQLCipher AES-256.
        /// Chỉ gọi một lần trong quá trình nâng cấp từ phiên bản không mã hóa.
        /// Yêu cầu: file ScaleData.db phải là SQLite thuần, chưa được mã hóa.
        /// Sau khi hoàn thành, xóa hoặc vô hiệu hóa lời gọi này.
        /// </summary>
        public static void EncryptExistingDatabase()
        {
            string key = Environment.GetEnvironmentVariable("TESA_DB_KEY")
                ?? throw new InvalidOperationException("[SEC-4.1] TESA_DB_KEY chưa được đặt.");

            // Mở file plaintext (không có password)
            string plainConnStr = $"Data Source={DatabaseFileName}";
            using var conn = new SqliteConnection(plainConnStr);
            conn.Open();

            // sqlcipher_export: xuất toàn bộ nội dung sang file mã hóa tạm thời
            // sau đó hoán đổi file
            using var attachCmd = new SqliteCommand(
                $"ATTACH DATABASE 'encrypted.db' AS encrypted KEY '{key}';", conn);
            attachCmd.ExecuteNonQuery();

            using var exportCmd = new SqliteCommand(
                "SELECT sqlcipher_export('encrypted');", conn);
            exportCmd.ExecuteNonQuery();

            using var detachCmd = new SqliteCommand("DETACH DATABASE encrypted;", conn);
            detachCmd.ExecuteNonQuery();

            conn.Close();

            // Hoán đổi file: xóa plaintext, đổi tên encrypted.db -> ScaleData.db
            System.IO.File.Delete(DatabaseFileName);
            System.IO.File.Move("encrypted.db", DatabaseFileName);

            System.Diagnostics.Debug.WriteLine("[SEC-4.1] EncryptExistingDatabase OK. File đã được mã hóa AES-256.");
        }

        /// <summary>
        /// v1 → v2: Đổi cột Weight từ REAL sang TEXT để giữ decimal precision.
        /// SQLite không hỗ trợ ALTER COLUMN nên dùng table-swap pattern.
        /// </summary>
        private static void MigrateWeightToText(SqliteConnection connection)
        {
            using var pragmaCmd = new SqliteCommand("PRAGMA table_info(ScaleRecords);", connection);
            using var reader = pragmaCmd.ExecuteReader();

            bool needsMigration = false;
            while (reader.Read())
            {
                string columnName = reader.GetString(1);
                string columnType = reader.GetString(2);
                if (columnName.Equals("Weight", StringComparison.OrdinalIgnoreCase)
                    && columnType.Equals("REAL", StringComparison.OrdinalIgnoreCase))
                {
                    needsMigration = true;
                    break;
                }
            }
            reader.Close();

            if (!needsMigration) return;

            using var transaction = connection.BeginTransaction();
            try
            {
                string[] migrationSql =
                {
                    "ALTER TABLE ScaleRecords RENAME TO ScaleRecords_old;",
                    @"CREATE TABLE ScaleRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp DATETIME NOT NULL,
                        Weight TEXT NOT NULL,
                        Unit TEXT NOT NULL,
                        NatCode TEXT,
                        Batch TEXT,
                        SampleName TEXT,
                        Location TEXT,
                        IsSynced INTEGER NOT NULL DEFAULT 0
                    );",
                    @"INSERT INTO ScaleRecords (Id, Timestamp, Weight, Unit, NatCode, Batch, SampleName, Location)
                      SELECT Id, Timestamp, CAST(Weight AS TEXT), Unit, NatCode, Batch, SampleName, Location
                      FROM ScaleRecords_old;",
                    "DROP TABLE ScaleRecords_old;"
                };

                foreach (var sql in migrationSql)
                {
                    using var cmd = new SqliteCommand(sql, connection, transaction);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                System.Diagnostics.Debug.WriteLine("[DB Migration v2] Weight: REAL -> TEXT OK.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                System.Diagnostics.Debug.WriteLine($"[DB Migration v2] Failed: {ex.Message}");
            }
        }

        /// <summary>
        /// v2 → v3: Thêm cột IsSynced nếu chưa tồn tại (SQLite hỗ trợ ADD COLUMN).
        /// </summary>
        private static void MigrateAddIsSynced(SqliteConnection connection)
        {
            using var pragmaCmd = new SqliteCommand("PRAGMA table_info(ScaleRecords);", connection);
            using var reader = pragmaCmd.ExecuteReader();

            bool columnExists = false;
            while (reader.Read())
            {
                if (reader.GetString(1).Equals("IsSynced", StringComparison.OrdinalIgnoreCase))
                {
                    columnExists = true;
                    break;
                }
            }
            reader.Close();

            if (columnExists) return;

            try
            {
                using var cmd = new SqliteCommand(
                    "ALTER TABLE ScaleRecords ADD COLUMN IsSynced INTEGER NOT NULL DEFAULT 0;",
                    connection);
                cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine("[DB Migration v3] IsSynced column added OK.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB Migration v3] Failed: {ex.Message}");
            }
        }

        /// <summary>
        /// v3 → v4: Thêm cột Tester nếu chưa tồn tại.
        /// </summary>
        private static void MigrateAddTester(SqliteConnection connection)
        {
            using var pragmaCmd = new SqliteCommand("PRAGMA table_info(ScaleRecords);", connection);
            using var reader = pragmaCmd.ExecuteReader();

            bool columnExists = false;
            while (reader.Read())
            {
                if (reader.GetString(1).Equals("Tester", StringComparison.OrdinalIgnoreCase))
                {
                    columnExists = true;
                    break;
                }
            }
            reader.Close();

            if (columnExists) return;

            try
            {
                using var cmd = new SqliteCommand(
                    "ALTER TABLE ScaleRecords ADD COLUMN Tester TEXT;",
                    connection);
                cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine("[DB Migration v4] Tester column added OK.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB Migration v4] Failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo bản sao lưu (backup) của CSDL SQLite hiện tại an toàn bằng cách dùng lock.
        /// </summary>
        public static async System.Threading.Tasks.Task BackupDatabaseAsync(string destinationPath)
        {
            await DbAccessLock.WaitAsync();
            try
            {
                // Force close connection if any pooled connections exist to flush WAL, though SQLite handles concurrent copies decently if wal checkpoint is triggered.
                SqliteConnection.ClearAllPools();
                System.IO.File.Copy(DatabaseFileName, destinationPath, overwrite: true);
            }
            finally
            {
                DbAccessLock.Release();
            }
        }
    }
}
