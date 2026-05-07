using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace Test
{
    public class CsvExportService
    {
        public void Export(string filePath, List<ScaleRecord> records)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }
    }
}
