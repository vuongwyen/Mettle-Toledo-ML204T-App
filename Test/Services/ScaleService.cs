using System;
using System.Threading.Tasks;

namespace Test.Services
{
    public class ScaleService : IDisposable
    {
        private readonly ConnectionManager _connectionManager;
        
        public event Action<ScaleData> OnDataReceived;
        public event Action<bool, bool> OnStatusChanged;

        public ScaleService()
        {
            _connectionManager = new ConnectionManager();
            _connectionManager.OnDataReceived += HandleRawData;
            _connectionManager.OnStateChanged += (connected, reconnecting) => OnStatusChanged?.Invoke(connected, reconnecting);
        }

        private void HandleRawData(string raw)
        {
            var data = MtSicsParser.Parse(raw);
            if (data.HasValue)
            {
                OnDataReceived?.Invoke(data.Value);
            }
        }

        public async Task ConnectAsync(string ip, int port)
        {
            await _connectionManager.ConnectAsync(ip, port);
        }

        public void Disconnect()
        {
            _connectionManager.Disconnect();
        }

        public ScaleInfo? ConnectedScale => _connectionManager.ConnectedScale;
        public bool IsConnected => _connectionManager.IsConnected;

        public void Dispose()
        {
            _connectionManager?.Dispose();
        }
    }
}
