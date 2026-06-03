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
