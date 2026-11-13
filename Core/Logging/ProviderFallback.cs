using System.Collections.Concurrent;

namespace Core.Logging;

/// <summary>Prints, once per provider and reason, why a configured logging provider isn't used.</summary>
public static class ProviderFallback
{
    private static readonly ConcurrentDictionary<string, bool> _reported = new();

    public static void Report(string provider, string? reason, string fallback)
    {
        var message = $"[Logging] Provider '{provider}' not used: {reason ?? "unavailable"}. Using '{fallback}' instead.";
        if (_reported.TryAdd(message, true))
        {
            System.Console.Error.WriteLine(message);
        }
    }
}
