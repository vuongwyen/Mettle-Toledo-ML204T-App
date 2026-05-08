using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Test.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        // SEC-02 FIX: Key is never stored in source code.
        // Set via: [System Environment] → Variable name: TESA_DB_KEY
        // For dev: $Env:TESA_DB_KEY = "YourStrongPassphrase" (PowerShell)
        private static string DbPassword =>
            System.Environment.GetEnvironmentVariable("TESA_DB_KEY")
            ?? throw new InvalidOperationException(
                "Database encryption key is missing. " +
                "Set the 'TESA_DB_KEY' environment variable and restart the application.");

        public DatabaseService(string dbPath = "scale_data.db")
        {
            // Kết nối SQLite với SQLCipher
            // Password=... kích hoạt AES-256 mã hóa toàn cục file .db
            _connectionString = $"Data Source={dbPath};Password={DbPassword};";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Weights (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Weight DECIMAL(18,4),
                        Unit TEXT,
                        NatCode TEXT,
                        Batch TEXT,
                        SampleName TEXT,
                        Location TEXT
                    );";
                command.ExecuteNonQuery();
            }
        }

        public void InsertRecord(ScaleRecord record)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Weights (Weight, Unit, NatCode, Batch, SampleName, Location)
                    VALUES ($weight, $unit, $nat, $batch, $sample, $loc)";
                
                cmd.Parameters.AddWithValue("$weight", record.Weight);
                cmd.Parameters.AddWithValue("$unit", record.Unit);
                cmd.Parameters.AddWithValue("$nat", record.NatCode);
                cmd.Parameters.AddWithValue("$batch", record.Batch);
                cmd.Parameters.AddWithValue("$sample", record.SampleName);
                cmd.Parameters.AddWithValue("$loc", record.Location);
                
                cmd.ExecuteNonQuery();
            }
        }

        public int GetTodayCount()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Weights WHERE date(Timestamp) = date('now')";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public decimal GetBatchTotal(string batch)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT SUM(Weight) FROM Weights WHERE Batch = $batch";
                cmd.Parameters.AddWithValue("$batch", batch);
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }
        public int GetRecordCount(string filter = "")
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                if (string.IsNullOrWhiteSpace(filter))
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Weights";
                }
                else
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Weights WHERE NatCode LIKE $f OR Batch LIKE $f OR SampleName LIKE $f";
                    cmd.Parameters.AddWithValue("$f", $"%{filter}%");
                }
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public List<ScaleRecord> GetRecords(int offset, int limit, string filter = "")
        {
            var records = new List<ScaleRecord>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                string sql = "SELECT * FROM Weights ";
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    sql += "WHERE NatCode LIKE $f OR Batch LIKE $f OR SampleName LIKE $f ";
                    cmd.Parameters.AddWithValue("$f", $"%{filter}%");
                }
                sql += "ORDER BY Timestamp DESC LIMIT $limit OFFSET $offset";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("$limit", limit);
                cmd.Parameters.AddWithValue("$offset", offset);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(new ScaleRecord
                        {
                            Id = reader.GetInt64(0),
                            Timestamp = reader.GetDateTime(1),
                            Weight = reader.GetDecimal(2),
                            Unit = reader.GetString(3),
                            NatCode = reader.GetString(4),
                            Batch = reader.GetString(5),
                            SampleName = reader.GetString(6),
                            Location = reader.GetString(7)
                        });
                    }
                }
            }
            return records;
        }
    }
}
