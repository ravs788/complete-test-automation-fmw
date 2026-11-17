using Core.Composition;

namespace Core.Logging;

/// <summary>
/// Creates the results publisher selected in <c>providers.json</c> (<c>ResultsPublisher</c>, default: same as
/// <c>Logging</c>), from the providers registered with <see cref="Framework"/> (R40). Falls back to console if the
/// selected provider isn't registered or is unavailable.
/// </summary>
public static class ResultsPublisherFactory
{
    private static readonly Lazy<IResultsPublisher> _publisher = new(
        () => Create(Framework.Providers, Framework.Settings.EffectiveResultsPublisher, LoggingConfigProvider.Current),
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>Returns the results publisher, created on first use and then reused for the rest of the test run.</summary>
    public static IResultsPublisher Create() => _publisher.Value;

    /// <summary>
    /// Creates the publisher called <paramref name="provider"/>; if it isn't registered or can't be used, says so once
    /// and uses console.
    /// </summary>
    public static IResultsPublisher Create(ProviderRegistry registry, string provider, LoggingConfig cfg)
    {
        provider = (provider ?? string.Empty).Trim();
        if (provider.Length > 0 && !IsConsole(provider))
        {
            if (!registry.TryGet<IResultsPublisherFactory>(provider, out var selected))
            {
                ProviderFallback.Report(provider, "no such provider", "console");
            }
            else if (selected.TryCreate(cfg, out var publisher, out var reason))
            {
                return publisher;
            }
            else
            {
                ProviderFallback.Report(provider, reason, "console");
            }
        }

        if (registry.TryGet<IResultsPublisherFactory>("console", out var console) &&
            console.TryCreate(cfg, out var consolePublisher, out _))
        {
            return consolePublisher;
        }

        // Last resort, in case the console provider was not registered.
        return new Console.ConsoleResultsPublisher();
    }

    private static bool IsConsole(string provider) => string.Equals(provider, "console", StringComparison.OrdinalIgnoreCase);
}
