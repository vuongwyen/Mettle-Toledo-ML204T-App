using System;
using System.Text.Json.Serialization;

namespace Test.Models
{
    public class ScaleRecord
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("natCode")]
        public string NatCode { get; set; }

        [JsonPropertyName("batch")]
        public string Batch { get; set; }

        [JsonPropertyName("sampleName")]
        public string SampleName { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("isSynced")]
        public bool IsSynced { get; set; }
    }

    public class SyncPayload<T>
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("syncTime")]
        public DateTimeOffset SyncTime { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
