using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class ConnectionManager : IDisposable
    {
        private TcpClient? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private CancellationTokenSource? _cts;
        
        private string _ip = string.Empty;
        private int _port = 0;
        private bool _isManualDisconnect = false;
        private bool _isReconnecting = false;

        public bool IsConnected    => _client != null && _client.Connected;
        public bool IsReconnecting => _isReconnecting;

        // isConnected, isReconnecting
        public event Action<bool, bool>? OnStateChanged;
        public event Action<string>? OnDataReceived;


        public async Task ConnectAsync(string ip, int port)
        {
            _ip = ip;
            _port = port;
            _isManualDisconnect = false;
            await EstablishConnectionAsync();
        }


        private async Task EstablishConnectionAsync()
        {
            DisconnectInternal();
            _cts = new CancellationTokenSource();
            _connectedScale = null;

            try
            {
                _client = new TcpClient();

                // ── Bước 1: TCP connect với timeout 5 giây ────────────────
                using var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try
                {
                    await _client.ConnectAsync(_ip, _port, connectCts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"Không thể kết nối tới {_ip}:{_port} sau 5 giây (Host không phản hồi).");
                }

                var stream = _client.GetStream();
                _reader = new StreamReader(stream, Encoding.ASCII);
                _writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                // ── Bước 2: MT-SICS Handshake — Xác thực thiết bị ──────────
                // Ưu tiên lệnh I2 (Model & Capacity) và I4 (Serial Number)
                
                // 2a. Lấy Model & Capacity (Lệnh I2)
                await _writer.WriteLineAsync("I2");
                string? i2Response = await ReadLineWithTimeoutAsync(3000);
                
                if (string.IsNullOrEmpty(i2Response) || !i2Response.StartsWith("I2 A"))
                {
                    throw new InvalidOperationException(
                        $"Thiết bị tại {_ip}:{_port} không phản hồi lệnh nhận diện (I2).\n" +
                        $"Response: \"{i2Response ?? "(timeout)"}\"");
                }

                // 2b. Lấy Serial Number (Lệnh I4)
                await _writer.WriteLineAsync("I4");
                string? i4Response = await ReadLineWithTimeoutAsync(2000);

                // Parse thông tin
                _connectedScale = ParseScaleIdentification(i2Response, i4Response);
                _connectedScale.IpAddress = _ip;

                // ── Bước 3: Xác thực thành công ──────────────────────────
                _isReconnecting = false;
                OnStateChanged?.Invoke(true, false);
                
                // Bắt đầu gửi lệnh SIR để lấy cân nặng liên tục
                await _writer.WriteLineAsync("SIR");

                _ = Task.Run(() => ReadLoopAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                DisconnectInternal();
                if (!_isManualDisconnect)
                {
                    _lastError = ex.Message;
                    _isReconnecting = true;
                    OnStateChanged?.Invoke(false, true);
                    _ = Task.Run(() => ReconnectLoopAsync());
                }
            }
        }

        private async Task<string?> ReadLineWithTimeoutAsync(int timeoutMs)
        {
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            try
            {
                return await _reader!.ReadLineAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        private ScaleInfo ParseScaleIdentification(string i2, string? i4)
        {
            var info = new ScaleInfo();
            
            // Parse I2: I2 A "MODEL CAPACITY UNIT"
            // Ví dụ: I2 A "MS204S 220.0090 g"
            var i2Match = System.Text.RegularExpressions.Regex.Match(i2, "I2 A \"(.*)\"");
            if (i2Match.Success)
            {
                string fullInfo = i2Match.Groups[1].Value;
                info.Model = fullInfo; // Hoặc split ra nếu cần chi tiết hơn
            }

            // Parse I4: I4 A "SERIAL"
            if (!string.IsNullOrEmpty(i4))
            {
                var i4Match = System.Text.RegularExpressions.Regex.Match(i4, "I4 A \"(.*)\"");
                if (i4Match.Success)
                {
                    info.SerialNumber = i4Match.Groups[1].Value;
                }
            }

            return info;
        }

        private static bool IsMtSicsResponse(string? response)
        {
            if (string.IsNullOrWhiteSpace(response)) return false;

            // MT-SICS weight response: "S S 0.0000 g", "S D -0.0023 g", "S I", "S +"
            if (response.Length >= 3 && response.StartsWith("S "))
            {
                char status = response[2];
                return status == 'S' || status == 'D' || status == 'I'
                    || status == '+' || status == '-';
            }

            return response == "ES" || response.StartsWith("I2 A") || response.StartsWith("I4 A");
        }

        public ScaleInfo? ConnectedScale => _connectedScale;
        private ScaleInfo? _connectedScale;


        public string? LastError => _lastError;
        private string? _lastError;


        private async Task ReadLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && IsConnected)
                {
                    string? line = await _reader!.ReadLineAsync(token);
                    if (line == null) break; 
                    
                    OnDataReceived?.Invoke(line);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (!_isManualDisconnect && !token.IsCancellationRequested)
                {
                    OnStateChanged?.Invoke(false, true);
                    _ = Task.Run(() => ReconnectLoopAsync());
                }
                else if (_isManualDisconnect)
                {
                    DisconnectInternal();
                    OnStateChanged?.Invoke(false, false);
                }
            }
        }

        private async Task ReconnectLoopAsync()
        {
            await Task.Delay(3000); // Thử lại sau 3s
            if (!_isManualDisconnect)
            {
                await EstablishConnectionAsync();
            }
        }

        public void Disconnect()
        {
            _isManualDisconnect = true;
            _isReconnecting     = false;
            DisconnectInternal();
            OnStateChanged?.Invoke(false, false);
        }

        private void DisconnectInternal()
        {
            try { _cts?.Cancel(); } catch { }
            
            _writer?.Dispose();
            _reader?.Dispose();
            _client?.Dispose();
            
            _writer = null;
            _reader = null;
            _client = null;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
