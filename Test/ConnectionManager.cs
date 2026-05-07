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
        
        // isConnected, isReconnecting
        public event Action<bool, bool>? OnStateChanged;
        public event Action<string>? OnDataReceived;

        public bool IsConnected => _client != null && _client.Connected;

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
            
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_ip, _port);
                
                var stream = _client.GetStream();
                _reader = new StreamReader(stream, Encoding.ASCII);
                _writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
                
                OnStateChanged?.Invoke(true, false);

                await _writer.WriteLineAsync("SIR");
                
                _ = Task.Run(() => ReadLoopAsync(_cts.Token), _cts.Token);
            }
            catch (Exception)
            {
                if (!_isManualDisconnect)
                {
                    OnStateChanged?.Invoke(false, true);
                    _ = Task.Run(() => ReconnectLoopAsync());
                }
            }
        }

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
