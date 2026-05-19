using System;
using Microsoft.Data.Sqlite;

namespace Test
{
    public static class DatabaseHelper
    {
        private const string DatabaseFileName = "ScaleData.db";

        // [FIX N3] Schema version tracking — tăng khi có thay đổi schema
        private const int CurrentSchemaVersion = 2;

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
                        Location TEXT
                    );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Migrate DB cũ: nếu cột Weight đang là REAL thì đổi sang TEXT
                MigrateIfNeeded(connection);
            }
        }

        /// <summary>
        /// Xử lý migration cho DB đã tồn tại trước đó với cột Weight REAL.
        /// SQLite không hỗ trợ ALTER COLUMN, nên kiểm tra type qua pragma.
        /// Nếu cột đã là TEXT thì skip — không cần tạo lại bảng.
        /// </summary>
        private static void MigrateIfNeeded(SqliteConnection connection)
        {
            // Kiểm tra kiểu cột Weight hiện tại
            using var pragmaCmd = new SqliteCommand("PRAGMA table_info(ScaleRecords);", connection);
            using var reader = pragmaCmd.ExecuteReader();

            bool needsMigration = false;
            while (reader.Read())
            {
                string columnName = reader.GetString(1); // name
                string columnType = reader.GetString(2); // type
                if (columnName.Equals("Weight", StringComparison.OrdinalIgnoreCase)
                    && columnType.Equals("REAL", StringComparison.OrdinalIgnoreCase))
                {
                    needsMigration = true;
                    break;
                }
            }
            reader.Close();

            if (!needsMigration) return;

            // SQLite không hỗ trợ ALTER COLUMN → tạo bảng mới, copy data, swap
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
                        Location TEXT
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
                System.Diagnostics.Debug.WriteLine("[DB Migration] Weight column migrated from REAL to TEXT successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                System.Diagnostics.Debug.WriteLine($"[DB Migration] Failed: {ex.Message}");
                // Không throw — app vẫn hoạt động được với REAL, chỉ giảm precision
            }
        }
    }
}
