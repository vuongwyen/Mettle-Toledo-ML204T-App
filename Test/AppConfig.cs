using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace Test
{
    public class AppConfig
    {
        private static readonly string ConfigFilePath = Path.Combine(Application.StartupPath, "appsettings.json");

        [JsonPropertyName("apiServerUrl")]
        public string ApiServerUrl { get; set; } = "http://172.29.49.36:5000";

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = "SYNC_SECRET_123456";

        [JsonPropertyName("adminPassword")]
        public string AdminPassword { get; set; } = "admin123";

        [JsonPropertyName("autoBackupIntervalHours")]
        public int AutoBackupIntervalHours { get; set; } = 4;

        /// <summary>
        /// Tên định danh máy trạm gửi lên Server (dùng để phân biệt thiết bị).
        /// Mặc định lấy tên máy tính hiện tại. Admin có thể đặt tên tuỳ ý trong tab Cài đặt.
        /// </summary>
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = Environment.MachineName;

        /// <summary>
        /// Đường dẫn tới thư mục Dropbox cục bộ dùng làm đích lưu file backup tự động.
        /// Dropbox app sẽ tự đồng bộ lên mây. Để trống = lưu trong thư mục Backups/ mặc định.
        /// </summary>
        [JsonPropertyName("dropboxFolderPath")]
        public string DropboxFolderPath { get; set; } = "";

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppConfig] Error loading: {ex.Message}");
            }
            
            // Return default config if file doesn't exist or errors out
            var defaultConfig = new AppConfig();
            defaultConfig.Save();
            return defaultConfig;
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppConfig] Error saving: {ex.Message}");
            }
        }
    }
}
