using System.Collections.Concurrent;
using Core.Composition;

namespace Core.Logging;

/// <summary>
/// Creates loggers with the provider selected in <c>providers.json</c> (<c>Logging</c>), from the providers registered
/// with <see cref="Framework"/> (R40). Falls back to console if the selected provider isn't registered or is unavailable.
/// </summary>
public static class LoggingServiceFactory
{
    private static readonly ConcurrentDictionary<string, Lazy<ILoggingService>> _loggers = new();

    /// <summary>
    /// Returns the logger for <paramref name="indexFormat"/>, created on first use with the configured provider and
    /// then reused for the rest of the test run (the services are thread-safe).
    /// </summary>
    public static ILoggingService CreateLogger(string indexFormat) =>
        _loggers.GetOrAdd(indexFormat, format => new Lazy<ILoggingService>(() =>
            Create(Framework.Providers, Framework.Settings.Logging, LoggingConfigProvider.Current, format))).Value;

    /// <summary>
    /// Creates a logger with the provider called <paramref name="provider"/>; if it isn't registered or can't be used
    /// (e.g. Elasticsearch unreachable), says so once and uses console.
    /// </summary>
    public static ILoggingService Create(ProviderRegistry registry, string provider, LoggingConfig cfg, string indexFormat)
    {
        provider = (provider ?? string.Empty).Trim();
        if (provider.Length > 0 && !IsConsole(provider))
        {
            if (!registry.TryGet<ILoggingProviderFactory>(provider, out var selected))
            {
                ProviderFallback.Report(provider, "no such provider", "console");
            }
            else if (selected.TryCreate(cfg, indexFormat, out var service, out var reason))
            {
                return service;
            }
            else
            {
                ProviderFallback.Report(provider, reason, "console");
            }
        }

        if (registry.TryGet<ILoggingProviderFactory>("console", out var console) &&
            console.TryCreate(cfg, indexFormat, out var consoleService, out _))
        {
            return consoleService;
        }

        // Last resort, in case the console provider was not registered.
        var fallback = new Console.ConsoleLoggingService();
        fallback.Configure(indexFormat);
        return fallback;
    }

    private static bool IsConsole(string provider) => string.Equals(provider, "console", StringComparison.OrdinalIgnoreCase);
}
