using System.Collections.Concurrent;
using Core.Security;

namespace Core.Logging;

/// <summary>Prints, once per provider and reason, why a configured provider (logging, reporting, …) isn't used.</summary>
public static class ProviderFallback
{
    private static readonly ConcurrentDictionary<string, bool> _reported = new();

    public static void Report(string provider, string? reason, string fallback, string area = "Logging")
    {
        var message = DiagnosticRedactor.Redact($"[{area}] Provider '{provider}' not used: {reason ?? "unavailable"}. Using '{fallback}' instead.");
        if (_reported.TryAdd(message, true))
        {
            System.Console.Error.WriteLine(message);
        }
    }
}
