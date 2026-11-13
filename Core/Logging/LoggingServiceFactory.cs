using System.Collections.Concurrent;

namespace Core.Logging;

/// <summary>
/// Generic selector for logging providers. Uses pluggable factories (ILoggingProviderFactory)
/// discovered via reflection, so no direct dependency on any specific provider.
/// </summary>
public static class LoggingServiceFactory
{
    private static readonly ConcurrentDictionary<string, Lazy<ILoggingService>> _loggers = new();

    /// <summary>
    /// Returns the logger for <paramref name="indexFormat"/>, created on first use with the configured provider and
    /// then reused for the rest of the test run (the services are thread-safe).
    /// Falls back to another available provider (preferably "console") if the chosen one is unavailable.
    /// </summary>
    public static ILoggingService CreateLogger(string indexFormat) =>
        _loggers.GetOrAdd(indexFormat, format => new Lazy<ILoggingService>(() => Create(format))).Value;

    private static ILoggingService Create(string indexFormat)
    {
        var cfg = LoggingConfigProvider.Current;
        var factories = DiscoverFactories();

        // Try explicitly selected provider first
        var provider = (cfg.Provider ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(provider))
        {
            var selected = factories.FirstOrDefault(f =>
                string.Equals(f.Name, provider, StringComparison.OrdinalIgnoreCase));

            if (selected == null)
            {
                ProviderFallback.Report(provider, "no such provider", "console");
            }
            else if (selected.TryCreate(cfg, indexFormat, out var svc, out var reason))
            {
                return svc;
            }
            else
            {
                ProviderFallback.Report(provider, reason, "console");
            }
        }

        // Prefer console if available
        var console = factories.FirstOrDefault(f =>
            string.Equals(f.Name, "console", StringComparison.OrdinalIgnoreCase));
        if (console != null && console.TryCreate(cfg, indexFormat, out var consoleSvc, out _))
        {
            return consoleSvc;
        }

        // Otherwise, try any available provider
        foreach (var f in factories)
        {
            if (f.TryCreate(cfg, indexFormat, out var svc, out _))
            {
                return svc;
            }
        }

        // Absolute last resort: direct console logger (in case no factory type was discovered)
        var fallback = new Console.ConsoleLoggingService();
        fallback.Configure(indexFormat);
        return fallback;
    }

    private static List<ILoggingProviderFactory> DiscoverFactories()
    {
        var asm = typeof(ILoggingProviderFactory).Assembly;
        var types = asm
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(ILoggingProviderFactory).IsAssignableFrom(t));

        var list = new List<ILoggingProviderFactory>();
        foreach (var t in types)
        {
            try
            {
                if (Activator.CreateInstance(t) is ILoggingProviderFactory instance)
                    list.Add(instance);
            }
            catch
            {
                // ignore load/activation failures
            }
        }

        return list;
    }
}
