using System;
using System.Text.Json.Serialization;

namespace Test.Models
{
    public class ScaleRecord
    {
        [JsonPropertyName("recordId")]
        public Guid Id { get; set; }

        [JsonPropertyName("testedAt")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("weightValue")]
        public decimal Weight { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("nart")]
        public string NatCode { get; set; }

        [JsonPropertyName("batchCode")]
        public string Batch { get; set; }

        [JsonPropertyName("sampleName")]
        public string SampleName { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("tester")]
        public string Tester { get; set; }

        [JsonPropertyName("isSynced")]
        public bool IsSynced { get; set; }
    }

    public class SyncPayload<T>
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("appId")]
        public string AppId { get; set; } = "Scale-App-001";

        [JsonPropertyName("appType")]
        public string AppType { get; set; } = "Scale";

        [JsonPropertyName("syncTime")]
        public DateTimeOffset SyncTime { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    public class SyncRecordDto
    {
        [JsonPropertyName("recordId")]
        public Guid RecordId { get; set; }

        [JsonPropertyName("testedAt")]
        public DateTimeOffset TestedAt { get; set; }

        [JsonPropertyName("payload")]
        public object Payload { get; set; }
    }
}
