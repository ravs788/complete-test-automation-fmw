using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Core.Logging.Elastic;

/// <summary>
/// Checks, once per URL per test run, that the server at the configured URL is an Elasticsearch the client can
/// write to: it answers <c>GET /</c> with 2xx using the configured credentials, reports a version, is 8.x or later,
/// and is not OpenSearch. Anything else makes the framework fall back to the console with the reason.
/// </summary>
public static class ElasticConnectivity
{
    private static readonly ConcurrentDictionary<string, Lazy<ElasticProbeResult>> _results = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>True when <see cref="Check"/> finds the server usable.</summary>
    public static bool IsUsable(string? elasticUrl, string? username = null, string? password = null, TimeSpan? timeout = null) =>
        Check(elasticUrl, username, password, timeout).IsUsable;

    /// <summary>Probes the server (cached per URL for the rest of the run) and explains why it can't be used.</summary>
    public static ElasticProbeResult Check(string? elasticUrl, string? username = null, string? password = null, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(elasticUrl))
        {
            return ElasticProbeResult.NotUsable("no Elastic URL is configured");
        }

        return _results.GetOrAdd(elasticUrl, url => new Lazy<ElasticProbeResult>(() => Probe(url, username, password, timeout))).Value;
    }

    /// <summary>Decides from the answer to <c>GET /</c> whether the server can be used. Pure, so it can be unit tested.</summary>
    public static ElasticProbeResult Evaluate(int statusCode, string? body)
    {
        if (statusCode is 401 or 403)
        {
            return ElasticProbeResult.NotUsable(
                $"authentication failed (HTTP {statusCode}); check ELASTIC_USERNAME / ELASTIC_PASSWORD (the user needs the 'monitor' cluster privilege)");
        }

        if (statusCode is < 200 or >= 300)
        {
            return ElasticProbeResult.NotUsable($"the server answered HTTP {statusCode}");
        }

        string? distribution = null;
        string? number = null;
        try
        {
            using var json = JsonDocument.Parse(body ?? string.Empty);
            if (json.RootElement.TryGetProperty("version", out var version) && version.ValueKind == JsonValueKind.Object)
            {
                distribution = version.TryGetProperty("distribution", out var d) ? d.GetString() : null;
                number = version.TryGetProperty("number", out var n) ? n.GetString() : null;
            }
        }
        catch (JsonException)
        {
            // Not JSON: handled below.
        }

        if (string.Equals(distribution, "opensearch", StringComparison.OrdinalIgnoreCase))
        {
            return ElasticProbeResult.NotUsable(
                $"the server is OpenSearch {number}, which Elastic's 8.x client can't write to; use Elasticsearch 8 (docker compose up -d)");
        }

        if (number is null || !int.TryParse(number.Split('.')[0], out var major))
        {
            return ElasticProbeResult.NotUsable("the server didn't answer like Elasticsearch (no version in GET /)");
        }

        return major >= 8
            ? ElasticProbeResult.Usable
            : ElasticProbeResult.NotUsable($"Elasticsearch {number} is too old; the client needs 8.x or later");
    }

    private static ElasticProbeResult Probe(string elasticUrl, string? username, string? password, TimeSpan? timeout)
    {
        try
        {
            using var handler = new HttpClientHandler
            {
                // Allow self-signed certs for local/dev scenarios. Secure profiles can harden this.
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var http = new HttpClient(handler)
            {
                Timeout = timeout ?? TimeSpan.FromMilliseconds(750)
            };

            using var req = new HttpRequestMessage(HttpMethod.Get, elasticUrl);
            if (!string.IsNullOrEmpty(username))
            {
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password ?? string.Empty}"));
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
            }

            using var resp = http.Send(req);
            using var reader = new StreamReader(resp.Content.ReadAsStream());
            return Evaluate((int)resp.StatusCode, reader.ReadToEnd());
        }
        catch (Exception ex)
        {
            return ElasticProbeResult.NotUsable($"unreachable at {elasticUrl} ({ex.GetType().Name}: {ex.Message})");
        }
    }
}
