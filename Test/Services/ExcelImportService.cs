using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace Test.Services
{
    public class ExcelImportService
    {
        public List<ScaleRecord> Import(string filePath)
        {
            var records = new List<ScaleRecord>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed()?.RowsUsed();
                
                if (rows == null) return records;

                bool isFirstRow = true;
                Dictionary<string, int> headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var row in rows)
                {
                    if (isFirstRow)
                    {
                        int colCount = row.CellCount();
                        for (int i = 1; i <= colCount; i++)
                        {
                            var header = row.Cell(i).GetString().Trim();
                            if (!string.IsNullOrEmpty(header))
                            {
                                headerMap[header] = i;
                            }
                        }
                        isFirstRow = false;
                        continue;
                    }

                    try
                    {
                        var record = new ScaleRecord();

                        if (headerMap.TryGetValue("Timestamp", out int tsCol) && row.Cell(tsCol).TryGetValue(out DateTime ts))
                            record.Timestamp = ts;
                        else
                            record.Timestamp = DateTime.Now;

                        if (headerMap.TryGetValue("Weight", out int wCol) && row.Cell(wCol).TryGetValue(out double weightDouble))
                            record.Weight = (decimal)weightDouble;
                        else
                            continue; // Bỏ qua dòng nếu không có khối lượng

                        if (headerMap.TryGetValue("Unit", out int unitCol))
                        {
                            var u = row.Cell(unitCol).GetString();
                            record.Unit = string.IsNullOrEmpty(u) ? "g" : u;
                        }
                        else
                            record.Unit = "g";

                        if (headerMap.TryGetValue("NatCode", out int natCol))
                            record.NatCode = row.Cell(natCol).GetString() ?? string.Empty;

                        if (headerMap.TryGetValue("Batch", out int batchCol))
                            record.Batch = row.Cell(batchCol).GetString() ?? string.Empty;

                        if (headerMap.TryGetValue("SampleName", out int sampleCol))
                            record.SampleName = row.Cell(sampleCol).GetString() ?? string.Empty;

                        if (headerMap.TryGetValue("Location", out int locCol))
                            record.Location = row.Cell(locCol).GetString() ?? string.Empty;

                        records.Add(record);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error parsing Excel row: {ex.Message}");
                    }
                }
            }
            return records;
        }
    }
}
