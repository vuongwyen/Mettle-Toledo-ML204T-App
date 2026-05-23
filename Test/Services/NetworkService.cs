using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Test.Models;

namespace Test.Services
{
    /// <summary>
    /// Singleton service chịu trách nhiệm đẩy dữ liệu cân lên Central Server qua WLAN.
    /// Sử dụng IHttpClientFactory pattern (static HttpClient) để đúng chuẩn vòng đời .NET 10.
    /// </summary>
    public sealed class NetworkService
    {
        private static readonly Lazy<NetworkService> _instance =
            new(() => new NetworkService(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static NetworkService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        private NetworkService()
        {
            _baseUrl = Environment.GetEnvironmentVariable("SCALE_API_URL")
                       ?? "http://localhost:5000";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl.TrimEnd('/')),
                Timeout     = TimeSpan.FromSeconds(5)
            };

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition      = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented               = false
            };
        }

        /// <summary>
        /// Ping nhẹ tới /health để kiểm tra WLAN có kết nối được server hay không.
        /// </summary>
        public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, "/health");
                using var cts     = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Serialize danh sách ScaleRecord thành JSON và POST lên /api/scale/sync.
        /// Trả về danh sách các Id đã được server xác nhận sync thành công.
        /// </summary>
        /// <param name="records">Danh sách bản ghi cần đồng bộ.</param>
        /// <param name="deviceId">Định danh máy trạm (Edge PC).</param>
        /// <param name="cancellationToken">Token huỷ.</param>
        public async Task<List<Guid>> PushDataAsync(
            List<ScaleRecord> records,
            string deviceId,
            CancellationToken cancellationToken = default)
        {
            if (records == null || records.Count == 0)
                return new List<Guid>();

            var payload = new SyncPayload<List<ScaleRecord>>
            {
                DeviceId = deviceId,
                SyncTime = DateTimeOffset.UtcNow,
                Data     = records
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/scale/sync",
                    payload,
                    _jsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return new List<Guid>();

                // Server trả về danh sách Id đã lưu thành công
                var result = await response.Content.ReadFromJsonAsync<SyncResponse>(_jsonOptions, cancellationToken);
                return result?.SyncedIds ?? new List<Guid>();
            }
            catch (OperationCanceledException)
            {
                return new List<Guid>();
            }
            catch (HttpRequestException)
            {
                return new List<Guid>();
            }
        }
    }

    /// <summary>
    /// DTO nhận phản hồi từ server sau khi sync.
    /// </summary>
    internal sealed class SyncResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("syncedIds")]
        public List<Guid> SyncedIds { get; set; } = new();
    }
}
