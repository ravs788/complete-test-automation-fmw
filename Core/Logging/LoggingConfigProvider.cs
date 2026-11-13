using System.Text.Json;

namespace Core.Logging;

/// <summary>
/// Thread-safe, lazily loaded <see cref="LoggingConfig"/>: <c>logging-config.json</c> (repository root, found by
/// walking up from the current directory), then environment overrides: <c>LOGGING_PROVIDER</c>, <c>ELASTIC_URL</c>,
/// <c>ELASTIC_USERNAME</c>, <c>ELASTIC_PASSWORD</c>, <c>ELASTIC_CERT_FINGERPRINT</c>. Credentials belong in the
/// environment, not in the committed file.
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
        var elastic = cfg.Elastic ?? new ElasticSection();
        return cfg with
        {
            Provider = Env("LOGGING_PROVIDER") ?? cfg.Provider,
            Elastic = elastic with
            {
                Url = Env("ELASTIC_URL") ?? elastic.Url,
                Username = Env("ELASTIC_USERNAME") ?? elastic.Username,
                Password = Env("ELASTIC_PASSWORD") ?? elastic.Password,
                CertificateFingerprint = Env("ELASTIC_CERT_FINGERPRINT") ?? elastic.CertificateFingerprint
            },
            Console = cfg.Console ?? new ConsoleSection()
        };
    }

    private static string? Env(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : null;

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
