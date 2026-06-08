using System.Collections.Generic;
using ClosedXML.Excel;

namespace Test
{
    public class ExcelExportService
    {
        public void Export(string filePath, List<ScaleRecord> records)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Scale Data");

            // Header row
            string[] headers = { "ID", "Thời gian", "Khối lượng", "Đơn vị", "Mã NAT", "Lô hàng", "Tên mẫu", "Vị trí", "Người đo" };
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(1, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#009FE3"); // BrandBlue
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data rows
            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                int row = i + 2;

                ws.Cell(row, 1).Value = r.Id;
                ws.Cell(row, 2).Value = r.Timestamp;
                ws.Cell(row, 2).Style.NumberFormat.Format = "yyyy-mm-dd hh:mm:ss";
                ws.Cell(row, 3).Value = (double)r.Weight;
                ws.Cell(row, 3).Style.NumberFormat.Format = "0.0000";
                ws.Cell(row, 4).Value = r.Unit;
                ws.Cell(row, 5).Value = r.NatCode;
                ws.Cell(row, 6).Value = r.Batch;
                ws.Cell(row, 7).Value = r.SampleName;
                ws.Cell(row, 8).Value = r.Location;
                ws.Cell(row, 9).Value = r.Tester;

                // Alternating row color
                if (i % 2 == 1)
                {
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9"); // Light Grey/Blue
                }
            }

            // AutoFit all columns + freeze header
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            wb.SaveAs(filePath);
        }
    }
}
