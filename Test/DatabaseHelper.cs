using System;
using Microsoft.Data.Sqlite;

namespace Test
{
    public static class DatabaseHelper
    {
        private const string DatabaseFileName = "ScaleData.db";

        public static string GetConnectionString()
        {
            return $"Data Source={DatabaseFileName}";
        }

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS ScaleRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp DATETIME NOT NULL,
                        Weight REAL NOT NULL,
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
            }
        }
    }
}
