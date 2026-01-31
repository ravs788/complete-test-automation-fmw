using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Core.Utilities
{
    public class LoggingConfig
    {
        // Selected logging provider: "elastic" or "console"
        public string Provider { get; set; } = "elastic";

        // Nested sections for provider-specific settings
        public ElasticSection Elastic { get; set; } = new ElasticSection();
        public ConsoleSection Console { get; set; } = new ConsoleSection();
        public FileSection FileLogging { get; set; } = new FileSection();

        public static LoggingConfig Load()
        {
            string rootPath = FindRootPath();
            string configPath = Path.Combine(rootPath, "logging-config.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Logging config file not found at: {configPath}");
            }

            var json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var cfg = JsonSerializer.Deserialize<LoggingConfig>(json, options) ?? new LoggingConfig();

            // Ensure non-null sections
            cfg.Elastic ??= new ElasticSection();
            cfg.Console ??= new ConsoleSection();
            cfg.FileLogging ??= new FileSection();

            return cfg;
        }

        private static string FindRootPath()
        {
            string currentDir = Directory.GetCurrentDirectory();
            while (!Directory.EnumerateFiles(currentDir, "*.sln").Any())
            {
                var parent = Directory.GetParent(currentDir);
                if (parent == null || parent.FullName == currentDir) // reached the root
                    throw new FileNotFoundException("Solution file not found in any parent directory");
                currentDir = parent.FullName;
            }
            return currentDir;
        }
    }
}
