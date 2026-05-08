using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Test.Services
{
    public class DatabaseService
    {
        private string _connectionString;

        // SEC-02 v2: Windows DPAPI — Machine-bound key, zero end-user config.
        // Key is generated once, encrypted with Windows DPAPI, stored in Registry.
        // Only the same Windows user/machine can decrypt it.
        private static string DbPassword
        {
            get
            {
                const string regPath = @"SOFTWARE\TESA\ScaleApp";
                const string regKey  = "DbKey";

                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(regPath, writable: false);
                var encryptedBytes = key?.GetValue(regKey) as byte[];

                if (encryptedBytes is not null)
                {
                    // Decrypt using DPAPI — only works on the same user/machine
                    var plain = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(plain);
                }

                // First run: generate a strong random key, encrypt and persist it
                var rawKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                var rawBytes = Encoding.UTF8.GetBytes(rawKey);
                var protectedBytes = ProtectedData.Protect(rawBytes, null, DataProtectionScope.CurrentUser);

                using var newKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regPath);
                newKey.SetValue(regKey, protectedBytes, Microsoft.Win32.RegistryValueKind.Binary);

                return rawKey;
            }
        }

        public DatabaseService(string dbPath = "scale_data.db")
        {
            // Kết nối SQLite với SQLCipher
            // Password=... kích hoạt AES-256 mã hóa toàn cục file .db
            _connectionString = $"Data Source={dbPath};Password={DbPassword};";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Guard: nếu file tồn tại nhưng key sai (Error 26), backup rồi tạo lại.
            TryOpenOrReset();
            CreateSchema();
        }

        private void TryOpenOrReset()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open(); // Sẽ throw nếu key sai
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 26)
            {
                // Error 26: file is not a database — key mismatch hoặc file plain (unencrypted)
                // Backup file lỗi, xóa để tạo lại với key đúng
                var dbPath = new SqliteConnectionStringBuilder(_connectionString).DataSource;
                if (System.IO.File.Exists(dbPath))
                {
                    var backup = $"{dbPath}.bak_{DateTime.Now:yyyyMMdd_HHmmss}";
                    System.IO.File.Move(dbPath, backup);
                    // Xóa DPAPI key cũ trong Registry để sinh key mới gắn với file mới
                    Microsoft.Win32.Registry.CurrentUser
                        .OpenSubKey(@"SOFTWARE\TESA\ScaleApp", writable: true)
                        ?.DeleteValue("DbKey", throwOnMissingValue: false);
                }
                // _connectionString vẫn hợp lệ — DbPassword sẽ sinh key mới ở lần gọi tiếp theo
                _connectionString_reset(); // Cập nhật connection string với key DPAPI mới
            }
        }

        private void _connectionString_reset()
        {
            var builder = new SqliteConnectionStringBuilder(_connectionString)
            {
                Password = DbPassword // DbPassword generates a new key (Registry was cleared)
            };
            _connectionString = builder.ToString();
        }

        private void CreateSchema()
        {
            using var connection = new SqliteConnection(_connectionString);
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
