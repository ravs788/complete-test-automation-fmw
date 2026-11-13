using Core.Configuration;

namespace Core.Logging;

/// <summary>
/// Thread-safe, lazily loaded <see cref="LoggingConfig"/>: <c>logging-config.json</c> (repository root, found by
/// walking up from the current directory), then <c>LOGGING_*</c> environment variables (e.g. <c>LOGGING_PROVIDER</c>,
/// <c>LOGGING_Elastic__Url</c>), then the aliases <c>ELASTIC_URL</c>, <c>ELASTIC_USERNAME</c>, <c>ELASTIC_PASSWORD</c>,
/// <c>ELASTIC_CERT_FINGERPRINT</c>. Credentials belong in the environment, not in the committed file.
/// </summary>
public static class LoggingConfigProvider
{
    public const string FileName = "logging-config.json";
    public const string EnvPrefix = "LOGGING_";

    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["ELASTIC_URL"] = "Elastic:Url",
        ["ELASTIC_USERNAME"] = "Elastic:Username",
        ["ELASTIC_PASSWORD"] = "Elastic:Password",
        ["ELASTIC_CERT_FINGERPRINT"] = "Elastic:CertificateFingerprint",
    };

    private static readonly Lazy<LoggingConfig> _lazyConfig = new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    public static LoggingConfig Current => _lazyConfig.Value;

    private static LoggingConfig Load()
    {
        var cfg = SettingsLoader.Load<LoggingConfig>(FindRootPath(), FileName, EnvPrefix, Aliases);

        // A section set to null in the JSON would otherwise override the defaults.
        return cfg with { Elastic = cfg.Elastic ?? new ElasticSection() };
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
