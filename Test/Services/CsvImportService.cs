using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace Test.Services
{
    public class CsvImportService
    {
        public List<ScaleRecord> Import(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            var records = new List<ScaleRecord>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    try
                    {
                        var record = new ScaleRecord();

                        if (csv.TryGetField("Timestamp", out DateTime ts))
                            record.Timestamp = ts;
                        else
                            record.Timestamp = DateTime.Now;

                        if (csv.TryGetField("Weight", out decimal weight))
                            record.Weight = weight;
                        else
                            continue; // Bỏ qua dòng nếu không có khối lượng

                        csv.TryGetField("Unit", out string? unit);
                        record.Unit = string.IsNullOrEmpty(unit) ? "g" : unit;

                        csv.TryGetField("NatCode", out string? natCode);
                        record.NatCode = natCode ?? string.Empty;

                        csv.TryGetField("Batch", out string? batch);
                        record.Batch = batch ?? string.Empty;

                        csv.TryGetField("SampleName", out string? sampleName);
                        record.SampleName = sampleName ?? string.Empty;

                        csv.TryGetField("Location", out string? location);
                        record.Location = location ?? string.Empty;

                        records.Add(record);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error parsing CSV row: {ex.Message}");
                    }
                }
            }
            return records;
        }
    }
}
