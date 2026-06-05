using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test
{
    public class ScaleRecord
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Weight { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? NatCode { get; set; }
        public string? Batch { get; set; }
        public string? SampleName { get; set; }
        public string? Location { get; set; }
        public string? Tester { get; set; }
        
        [System.ComponentModel.Browsable(false)]
        public bool IsSynced { get; set; }
        
        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
