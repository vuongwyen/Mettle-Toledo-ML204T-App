using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Services
{
    /// <summary>
    /// [SEC-4.3] Singleton service quản lý JWT Access Token trong memory.
    /// Token không được ghi xuống disk hoặc Registry.
    /// Vòng đời: tồn tại trong RAM cho đến khi ứng dụng kết thúc hoặc gọi ClearToken().
    /// </summary>
    public sealed class AuthService
    {
        private static readonly Lazy<AuthService> _instance =
            new(() => new AuthService(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static AuthService Instance => _instance.Value;

        // Token được lưu trong private field — không serialize, không persist.
        private string? _accessToken;
        private readonly object _tokenLock = new();

        // Endpoint xác thực lấy từ env var — không hardcode.
        private readonly string _authUrl;

        private AuthService()
        {
            string apiUrl = Environment.GetEnvironmentVariable("SCALE_API_URL")
                ?? throw new InvalidOperationException(
                    "[SEC-4.3] Biến môi trường 'SCALE_API_URL' chưa được đặt.");

            _authUrl = apiUrl.TrimEnd('/') + "/api/auth/login";
        }

        /// <summary>
        /// Trả về Access Token hiện tại. Null nếu chưa đăng nhập.
        /// </summary>
        public string? GetToken()
        {
            lock (_tokenLock) return _accessToken;
        }

        /// <summary>
        /// Đặt token từ bên ngoài (dùng cho M2M hoặc test).
        /// </summary>
        public void SetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token không được null hoặc rỗng.", nameof(token));

            lock (_tokenLock) _accessToken = token;
        }

        /// <summary>
        /// Xóa token khỏi memory (gọi khi logout hoặc token hết hạn).
        /// </summary>
        public void ClearToken()
        {
            lock (_tokenLock) _accessToken = null;
        }

        /// <summary>
        /// Trả về true nếu hiện có token trong memory.
        /// Không xác thực token với server — chỉ kiểm tra null/empty.
        /// </summary>
        public bool HasToken()
        {
            lock (_tokenLock) return !string.IsNullOrEmpty(_accessToken);
        }

        /// <summary>
        /// [SEC-4.3] Đăng nhập bằng credentials, lưu JWT vào memory nếu thành công.
        /// Sử dụng một HttpClient độc lập (không phải _httpClient của NetworkService)
        /// để tránh dependency vòng tròn.
        /// </summary>
        /// <param name="username">Tên đăng nhập.</param>
        /// <param name="password">Mật khẩu. Không được log hoặc lưu trữ.</param>
        /// <param name="cancellationToken">Token huỷ.</param>
        /// <returns>True nếu đăng nhập thành công và token đã được lưu.</returns>
        public async Task<bool> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username không được rỗng.", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password không được rỗng.", nameof(password));

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            var payload = new { username, password };
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                var response = await client.PostAsJsonAsync(_authUrl, payload, jsonOptions, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[AuthService] Login thất bại — HTTP {(int)response.StatusCode}.");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>(
                    jsonOptions, cancellationToken);

                if (result == null || string.IsNullOrWhiteSpace(result.AccessToken))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[AuthService] Server trả về token rỗng hoặc không hợp lệ.");
                    return false;
                }

                SetToken(result.AccessToken);
                System.Diagnostics.Debug.WriteLine("[AuthService] Login thành công. Token đã được lưu vào memory.");
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] Lỗi mạng khi login: {ex.Message}");
                return false;
            }
        }

        // DTO nội bộ — ánh xạ phản hồi JSON từ endpoint /api/auth/login
        private sealed class AuthResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("accessToken")]
            public string? AccessToken { get; set; }
        }
    }

    /// <summary>
    /// [SEC-4.3] DelegatingHandler tự động đính kèm JWT Bearer Token vào mỗi request.
    /// Lấy token từ AuthService.Instance tại thời điểm gửi request (không cache).
    /// Nếu chưa có token, request vẫn được gửi đi (server sẽ từ chối với HTTP 401).
    /// </summary>
    internal sealed class BearerTokenHandler : DelegatingHandler
    {
        public BearerTokenHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string? token = AuthService.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                // Ghi đè header Authorization trên mỗi request — không dùng DefaultRequestHeaders
                // để tránh race condition khi token được refresh giữa chừng.
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    "[BearerTokenHandler] Cảnh báo: Token chưa có. Request gửi không có Authorization header.");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
