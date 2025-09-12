using System.IO;
using System.Text.Json;

namespace Core.Utilities
{
    // Generic loader to read the project's config.json into a strongly-typed settings object.
    public static class ConfigLoader
    {
        public static T Load<T>() where T : new()
        {
            var configPath = Path.Combine(System.AppContext.BaseDirectory, "config.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"config.json not found in test output directory: {configPath}");

            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
    }
}
