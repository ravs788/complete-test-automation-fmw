using System.Text.Json;

namespace Core.Logging;

/// <summary>
/// Thread-safe, lazily loaded <see cref="LoggingConfig"/> from <c>logging-config.json</c>, which lives at the
/// repository root (found by walking up from the current directory).
/// </summary>
public static class LoggingConfigProvider
{
    public const string FileName = "logging-config.json";

    private static readonly Lazy<LoggingConfig> _lazyConfig = new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    public static LoggingConfig Current => _lazyConfig.Value;

    private static LoggingConfig Load()
    {
        var path = Path.Combine(FindRootPath(), FileName);
        var cfg = JsonSerializer.Deserialize<LoggingConfig>(
                      File.ReadAllText(path),
                      new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new LoggingConfig();

        // A section set to null in the JSON would otherwise override the defaults.
        return cfg with
        {
            Elastic = cfg.Elastic ?? new ElasticSection(),
            Console = cfg.Console ?? new ConsoleSection()
        };
    }

    private static string FindRootPath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(currentDir, FileName)))
        {
            var parent = Directory.GetParent(currentDir)
                         ?? throw new FileNotFoundException($"{FileName} not found in any parent directory");
            currentDir = parent.FullName;
        }
        return currentDir;
    }
}
