using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Core.Utilities
{
    public record class LoggingConfig
    {
        // Selected logging provider: "elastic" or "console"
        public string Provider { get; init; } = "elastic";

        // Nested sections for provider-specific settings
        public ElasticSection Elastic { get; init; } = new ElasticSection();
        public ConsoleSection Console { get; init; } = new ConsoleSection();

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
            return cfg with
            {
                Elastic = cfg.Elastic ?? new ElasticSection(),
                Console = cfg.Console ?? new ConsoleSection()
            };
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
