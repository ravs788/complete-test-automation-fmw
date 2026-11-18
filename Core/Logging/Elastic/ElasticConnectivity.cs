using System.Collections.Concurrent;
using System.Text.Json;
using Core.Security;
using Elastic.Transport;
using HttpMethod = Elastic.Transport.HttpMethod;

namespace Core.Logging.Elastic;

/// <summary>
/// Checks, once per connection configuration per test run, that the server at the configured URL is an Elasticsearch the client can
/// write to: it answers <c>GET /</c> with 2xx using the configured credentials, reports a version, is 8.x or later,
/// and is not OpenSearch. Anything else makes the framework fall back to the console with the reason.
/// </summary>
public static class ElasticConnectivity
{
    private static readonly ConcurrentDictionary<ProbeKey, Lazy<ElasticProbeResult>> _results = new();

    private sealed record ProbeKey(string Url, string? Username, string? Password, ElasticServerChoices Profile, string? Fingerprint, TimeSpan? Timeout);

    /// <summary>True when <see cref="Check"/> finds the server usable.</summary>
    public static bool IsUsable(string? elasticUrl, string? username = null, string? password = null, TimeSpan? timeout = null) =>
        Check(elasticUrl, username, password, timeout).IsUsable;

    /// <summary>Probes the server (cached per connection configuration for the rest of the run) and explains why it can't be used.</summary>
    public static ElasticProbeResult Check(string? elasticUrl, string? username = null, string? password = null, TimeSpan? timeout = null,
        ElasticServerChoices profile = ElasticServerChoices.ON_CLOUD, string? certificateFingerprint = null)
    {
        if (string.IsNullOrWhiteSpace(elasticUrl))
        {
            return ElasticProbeResult.NotUsable("no Elastic URL is configured");
        }

        var key = new ProbeKey(elasticUrl, username, password, profile, certificateFingerprint, timeout);
        return _results.GetOrAdd(key, connection => new Lazy<ElasticProbeResult>(() => Probe(connection))).Value;
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

    private static ElasticProbeResult Probe(ProbeKey connection)
    {
        try
        {
            var config = new LoggingConfig
            {
                Elastic = new ElasticSection
                {
                    Url = connection.Url,
                    Username = connection.Username,
                    Password = connection.Password,
                    CertificateFingerprint = connection.Fingerprint
                }
            };
            // Reuse the actual client's trust policy, fingerprint handling and authentication.
            var client = ElasticClientFactory.Create(connection.Profile, config,
                connection.Timeout ?? TimeSpan.FromMilliseconds(750));
            var response = client.Transport.Request<StringResponse>(HttpMethod.GET, "/");
            return Evaluate(response.ApiCallDetails.HttpStatusCode ?? 0, response.Body);
        }
        catch (Exception ex)
        {
            return ElasticProbeResult.NotUsable(DiagnosticRedactor.Redact($"unreachable at {connection.Url} ({ex.GetType().Name}: {ex.Message})"));
        }
    }
}
