using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Test
{
    public class DataRepository
    {
        public void Insert(ScaleRecord record)
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
                    command.Parameters.AddWithValue("@Weight", record.Weight);
                    command.Parameters.AddWithValue("@Unit", record.Unit ?? "g");
                    command.Parameters.AddWithValue("@NatCode", string.IsNullOrEmpty(record.NatCode) ? (object)DBNull.Value : record.NatCode);
                    command.Parameters.AddWithValue("@Batch", string.IsNullOrEmpty(record.Batch) ? (object)DBNull.Value : record.Batch);
                    command.Parameters.AddWithValue("@SampleName", string.IsNullOrEmpty(record.SampleName) ? (object)DBNull.Value : record.SampleName);
                    command.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(record.Location) ? (object)DBNull.Value : record.Location);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<ScaleRecord> GetAll()
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
                            Weight = reader.GetDecimal(2),
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
    }
}
