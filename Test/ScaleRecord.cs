using System;

namespace Test
{
    public class ScaleRecord
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Weight { get; set; }
        public string Unit { get; set; }
        public string NatCode { get; set; }
        public string Batch { get; set; }
        public string SampleName { get; set; }
        public string Location { get; set; }
    }
}
