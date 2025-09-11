using System.IO;
using System.Text.Json;

namespace Core.Utilities
{

    public class ConfigManager : IConfigManager
    {
        public static string GetRootPath()
        {
            return Directory.GetCurrentDirectory();
        }

        public static string RootPath => GetRootPath();
        private static readonly ConfigManager _instance = new ConfigManager();
        public static IConfigManager Instance => _instance;

        // Remove the duplicate GetRootPath and RootPath definitions

        private ConfigSettings? _settings;
        private readonly object _lock = new();

        public ConfigSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    lock (_lock)
                    {
                        if (_settings == null)
                        {
                            var configPath = Path.Combine(System.AppContext.BaseDirectory, "config.json");
                            if (!File.Exists(configPath))
                                throw new FileNotFoundException($"config.json not found in test output directory: {configPath}");

                            var json = File.ReadAllText(configPath);
                            _settings = JsonSerializer.Deserialize<ConfigSettings>(json) ?? new ConfigSettings();
                        }
                    }
                }
                return _settings;
            }
        }
    }
}
