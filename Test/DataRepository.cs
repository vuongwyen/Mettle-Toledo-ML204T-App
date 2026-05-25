using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Test
{
    /// <summary>
    /// Handles all SQLite database operations for ScaleRecord persistence.
    /// Uses parameterized queries to ensure security against SQL injection.
    /// </summary>
    public class DataRepository
    {
        /// <summary>
        /// Inserts a new scale measurement into the database.
        /// </summary>
        public void Insert(ScaleRecord record)
        {
            // [R-02] Serialize concurrent SQLite access with DatabaseService background timer
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
            using (var connection = new SqliteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                string query = @"
                    INSERT INTO ScaleRecords (Timestamp, Weight, Unit, NatCode, Batch, SampleName, Location)
                    VALUES (@Timestamp, @Weight, @Unit, @NatCode, @Batch, @SampleName, @Location)";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Timestamp", record.Timestamp);
                    command.Parameters.AddWithValue("@Weight", record.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    command.Parameters.AddWithValue("@Unit", record.Unit ?? "g");
                    command.Parameters.AddWithValue("@NatCode", string.IsNullOrEmpty(record.NatCode) ? (object)DBNull.Value : record.NatCode);
                    command.Parameters.AddWithValue("@Batch", string.IsNullOrEmpty(record.Batch) ? (object)DBNull.Value : record.Batch);
                    command.Parameters.AddWithValue("@SampleName", string.IsNullOrEmpty(record.SampleName) ? (object)DBNull.Value : record.SampleName);
                    command.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(record.Location) ? (object)DBNull.Value : record.Location);
                    
                    command.ExecuteNonQuery();
                }
            }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        /// <summary>
        /// Inserts multiple scale measurements efficiently using a transaction.
        /// </summary>
        public void InsertBatch(IEnumerable<ScaleRecord> records)
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
            using (var connection = new SqliteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    string query = @"
                        INSERT INTO ScaleRecords (Timestamp, Weight, Unit, NatCode, Batch, SampleName, Location)
                        VALUES (@Timestamp, @Weight, @Unit, @NatCode, @Batch, @SampleName, @Location)";

                    using (var command = new SqliteCommand(query, connection, transaction))
                    {
                        command.Parameters.Add("@Timestamp", SqliteType.Text);
                        command.Parameters.Add("@Weight", SqliteType.Text);
                        command.Parameters.Add("@Unit", SqliteType.Text);
                        command.Parameters.Add("@NatCode", SqliteType.Text);
                        command.Parameters.Add("@Batch", SqliteType.Text);
                        command.Parameters.Add("@SampleName", SqliteType.Text);
                        command.Parameters.Add("@Location", SqliteType.Text);

                        foreach (var record in records)
                        {
                            command.Parameters["@Timestamp"].Value = record.Timestamp;
                            command.Parameters["@Weight"].Value = record.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            command.Parameters["@Unit"].Value = record.Unit ?? "g";
                            command.Parameters["@NatCode"].Value = string.IsNullOrEmpty(record.NatCode) ? (object)DBNull.Value : record.NatCode;
                            command.Parameters["@Batch"].Value = string.IsNullOrEmpty(record.Batch) ? (object)DBNull.Value : record.Batch;
                            command.Parameters["@SampleName"].Value = string.IsNullOrEmpty(record.SampleName) ? (object)DBNull.Value : record.SampleName;
                            command.Parameters["@Location"].Value = string.IsNullOrEmpty(record.Location) ? (object)DBNull.Value : record.Location;

                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        public List<ScaleRecord> GetAll()
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
            var records = new List<ScaleRecord>();
            using (var connection = new SqliteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT Id, Timestamp, Weight, Unit, NatCode, Batch, SampleName, Location FROM ScaleRecords ORDER BY Timestamp DESC";

                using (var command = new SqliteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(new ScaleRecord
                        {
                            Id = reader.GetInt64(0),
                            Timestamp = reader.GetDateTime(1),
                            Weight = decimal.Parse(reader.GetString(2), System.Globalization.CultureInfo.InvariantCulture),
                            Unit = reader.GetString(3),
                            NatCode = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Batch = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            SampleName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            Location = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                        });
                    }
                }
            }
            return records;
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }
        public int GetTodayCount()
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
            using (var connection = new SqliteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM ScaleRecords WHERE DATE(Timestamp) = DATE('now', 'localtime')";
                using (var command = new SqliteCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    return result == null ? 0 : (int)(long)result;
                }
            }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }

        public decimal GetBatchTotal(string batch)
        {
            DatabaseHelper.DbAccessLock.Wait();
            try
            {
            using (var connection = new SqliteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                string query = string.IsNullOrEmpty(batch)
                    ? "SELECT COALESCE(SUM(Weight), 0) FROM ScaleRecords WHERE DATE(Timestamp) = DATE('now', 'localtime')"
                    : "SELECT COALESCE(SUM(Weight), 0) FROM ScaleRecords WHERE Batch = @Batch AND DATE(Timestamp) = DATE('now', 'localtime')";

                using (var command = new SqliteCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(batch))
                        command.Parameters.AddWithValue("@Batch", batch);

                    var result = command.ExecuteScalar();
                    return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
                }
            }
            }
            finally
            {
                DatabaseHelper.DbAccessLock.Release();
            }
        }
    }
}
