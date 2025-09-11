using System.IO;
using System.Text.Json;

namespace Core.Utilities
{
    public class LoggingConfig
    {
        public string ElasticUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public static LoggingConfig Load()
        {
            // Find the root directory containing the solution file
            string rootPath = FindRootPath();

            string configPath = Path.Combine(rootPath, "logging-config.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Logging config file not found at: {configPath}");
            }

            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<LoggingConfig>(json);
        }

        private static string FindRootPath()
        {
            string currentDir = Directory.GetCurrentDirectory();
            while (!Directory.EnumerateFiles(currentDir, "*.sln").Any())
            {
                currentDir = Directory.GetParent(currentDir).FullName;
                if (currentDir == Directory.GetParent(currentDir).FullName) // reached the root
                    throw new FileNotFoundException("Solution file not found in any parent directory");
            }
            return currentDir;
        }
    }
}
