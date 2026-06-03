using System;

namespace Test
{
    public class ScaleRecord
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Weight { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string NatCode { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string SampleName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Tester { get; set; } = string.Empty;
    }
}
