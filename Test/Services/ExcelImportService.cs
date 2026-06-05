using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Test;

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

                        if (headerMap.TryGetValue("Thời gian", out int tsCol1) || headerMap.TryGetValue("Timestamp", out tsCol1))
                        {
                            if (row.Cell(tsCol1).TryGetValue(out DateTime ts))
                                record.Timestamp = ts;
                            else if (DateTime.TryParse(row.Cell(tsCol1).GetString(), out DateTime tsStr))
                                record.Timestamp = tsStr;
                            else
                                record.Timestamp = DateTime.Now;
                        }
                        else
                            record.Timestamp = DateTime.Now;

                        if (headerMap.TryGetValue("Khối lượng", out int wCol) || headerMap.TryGetValue("Weight", out wCol))
                        {
                            if (row.Cell(wCol).TryGetValue(out double weightDouble))
                                record.Weight = (decimal)weightDouble;
                            else if (double.TryParse(row.Cell(wCol).GetString(), out double wStr))
                                record.Weight = (decimal)wStr;
                            else
                                continue;
                        }
                        else
                            continue; // Bỏ qua dòng nếu không có khối lượng

                        if (headerMap.TryGetValue("Đơn vị", out int unitCol) || headerMap.TryGetValue("Unit", out unitCol))
                        {
                            var u = row.Cell(unitCol).GetString();
                            record.Unit = string.IsNullOrEmpty(u) ? "g" : u;
                        }
                        else
                            record.Unit = "g";

                        if (headerMap.TryGetValue("Mã NAT", out int natCol) || headerMap.TryGetValue("NatCode", out natCol))
                            record.NatCode = row.Cell(natCol).GetString() ?? string.Empty;

                        if (headerMap.TryGetValue("Lô hàng", out int batchCol) || headerMap.TryGetValue("Batch", out batchCol))
                            record.Batch = row.Cell(batchCol).GetString() ?? string.Empty;

                        if (headerMap.TryGetValue("Tên mẫu", out int sampleCol) || headerMap.TryGetValue("SampleName", out sampleCol))
                            record.SampleName = row.Cell(sampleCol).GetString() ?? string.Empty;

                        if (headerMap.TryGetValue("Vị trí", out int locCol) || headerMap.TryGetValue("Location", out locCol))
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
