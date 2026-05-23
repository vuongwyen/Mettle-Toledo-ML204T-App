using System;
using Microsoft.Data.Sqlite;

namespace Test
{
    public static class DatabaseHelper
    {
        private const string DatabaseFileName = "ScaleData.db";

        // [FIX N3] Schema version tracking — tăng khi có thay đổi schema
        private const int CurrentSchemaVersion = 3;

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
                        IsSynced INTEGER NOT NULL DEFAULT 0
                    );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Migrate DB cũ theo thứ tự
                MigrateWeightToText(connection);  // v1 -> v2: Weight REAL -> TEXT
                MigrateAddIsSynced(connection);    // v2 -> v3: Thêm cột IsSynced
            }
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
    }
}
