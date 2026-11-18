using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Core.Security;

/// <summary>Sanitizes framework diagnostics. It does not modify requests or binary attachments.</summary>
public static class DiagnosticRedactor
{
    private const string Mask = "[REDACTED]";
    private static readonly ConcurrentDictionary<string, byte> Secrets = new(StringComparer.Ordinal);
    private static readonly TimeSpan Limit = TimeSpan.FromMilliseconds(100);
    private static readonly Regex Url = new("[a-z][a-z0-9+.-]*://[^\\s\\\"'<>]+", RegexOptions.IgnoreCase, Limit);
    private static readonly Regex UserInfo = new(@"(://)[^/\s]+@", RegexOptions.None, Limit);
    private static readonly Regex QueryValue = new(@"([?&][^=&#\s]+)=([^&#\s]*)", RegexOptions.None, Limit);
    private static readonly Regex Headers = new(@"(?im)((?:authorization|proxy-authorization|cookie|set-cookie|x-api-key)\s*:\s*)[^\r\n]+", RegexOptions.None, Limit);
    private static readonly Regex Assignments = new("""
        (?i)(["']?(?:password|passwd|token|access_token|refresh_token|client_secret|secret|api[-_]?key|authorization|cookie)["']?\s*[:=]\s*)(\[REDACTED\]|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|[^\s,;&}\]]+)
        """, RegexOptions.None, Limit);

    /// <summary>Registers a configured secret so even unlabelled diagnostic occurrences are removed for this run.</summary>
    public static void RegisterSecret(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.StartsWith(SettingsScheme, StringComparison.OrdinalIgnoreCase)) return;
        Secrets.TryAdd(value, 0);
        Secrets.TryAdd(Uri.EscapeDataString(value), 0);
    }

    private const string SettingsScheme = "secret://";

    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value ?? "";
        try
        {
            var result = Url.Replace(value, match =>
            {
                var safe = UserInfo.Replace(match.Value, "$1" + Mask + "@");
                var fragment = safe.IndexOf('#');
                if (fragment >= 0) safe = safe[..fragment] + "#" + Mask;
                return QueryValue.Replace(safe, "$1=" + Mask);
            });
            result = QueryValue.Replace(result, "$1=" + Mask);
            result = Headers.Replace(result, "$1" + Mask);
            result = Assignments.Replace(result, "$1" + Mask);
            foreach (var secret in Secrets.Keys.OrderByDescending(s => s.Length))
            {
                result = result.Replace(secret, Mask, StringComparison.Ordinal);
            }
            return result;
        }
        catch (RegexMatchTimeoutException)
        {
            return "[Diagnostic omitted: redaction limit reached]";
        }
    }
}
