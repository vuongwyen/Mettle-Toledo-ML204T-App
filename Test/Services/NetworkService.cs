using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Test.Models;

namespace Test.Services
{
    /// <summary>
    /// Singleton service chịu trách nhiệm đẩy dữ liệu cân lên Central Server qua WLAN.
    /// [SEC-4.2] Chỉ chấp nhận HTTPS. Không có fallback HTTP.
    /// [SEC-4.3] Tự động đính kèm JWT Bearer Token qua BearerTokenHandler.
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
            // [SEC-4.2] Không có fallback. Biến môi trường bắt buộc phải có giá trị hợp lệ.
            string? rawUrl = Environment.GetEnvironmentVariable("SCALE_API_URL");

            if (string.IsNullOrWhiteSpace(rawUrl))
                throw new InvalidOperationException(
                    "[SEC-4.2] Biến môi trường 'SCALE_API_URL' chưa được đặt. " +
                    "Ứng dụng yêu cầu URL HTTPS hợp lệ để giao tiếp với Central Server.");

            if (!rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"[SEC-4.2] 'SCALE_API_URL' phải bắt đầu bằng 'https://'. " +
                    $"Giá trị hiện tại: '{rawUrl}'. " +
                    $"Kết nối HTTP không mã hóa bị từ chối theo chính sách Zero-Trust.");

            _baseUrl = rawUrl.TrimEnd('/');

            // [SEC-4.3] HttpClient được tạo với BearerTokenHandler trong pipeline.
            // Mỗi request sẽ tự động nhận header Authorization: Bearer {token}.
            _httpClient = new HttpClient(
                new BearerTokenHandler(new HttpClientHandler()))
            {
                BaseAddress = new Uri(_baseUrl),
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
        /// [R-03] Đính kèm X-Idempotency-Key để server nhận diện gói tin gửi lại (timeout).
        /// Trả về danh sách các Id đã được server xác nhận sync thành công.
        /// </summary>
        /// <param name="records">Danh sách bản ghi cần đồng bộ.</param>
        /// <param name="deviceId">Định danh máy trạm (Edge PC).</param>
        /// <param name="cancellationToken">Token huỷ.</param>
        public async Task<List<Guid>> PushDataAsync(
            List<Test.Models.ScaleRecord> records,
            string deviceId,
            CancellationToken cancellationToken = default)
        {
            if (records == null || records.Count == 0)
                return new List<Guid>();

            var payload = new SyncPayload<List<Test.Models.ScaleRecord>>
            {
                DeviceId = deviceId,
                SyncTime = DateTimeOffset.UtcNow,
                Data     = records
            };

            // [R-03] Idempotency key: xác định duy nhất theo tập records — không phụ thuộc thứ tự gửi
            string idempotencyKey = ComputeIdempotencyKey(records);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/api/scale/sync");
                request.Content = JsonContent.Create(payload, options: _jsonOptions);
                request.Headers.TryAddWithoutValidation("X-Idempotency-Key", idempotencyKey);

                var response = await _httpClient.SendAsync(request, cancellationToken);

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

        /// <summary>
        /// [R-03] Tính Idempotency Key xác định từ tập records.
        /// Cùng tập records (kể cả khác thứ tự) luôn sinh ra cùng key.
        /// </summary>
        private static string ComputeIdempotencyKey(List<Test.Models.ScaleRecord> records)
        {
            if (records.Count == 1)
                return records[0].Id.ToString();

            // Sort by Id để key không phụ thuộc thứ tự, concat, hash MD5 → Guid string
            var sortedConcat = string.Concat(
                records.OrderBy(r => r.Id).Select(r => r.Id.ToString("N")));

            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(sortedConcat));
            return new Guid(hash).ToString();
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
