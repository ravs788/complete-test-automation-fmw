namespace Core.Logging;

/// <summary>
/// Elasticsearch connection settings. Never commit real credentials: leave them empty in
/// <c>logging-config.json</c> and set <c>ELASTIC_USERNAME</c> / <c>ELASTIC_PASSWORD</c> instead.
/// </summary>
public sealed record ElasticSection
{
    public string? Url { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }

    /// <summary>
    /// SHA-256 fingerprint of the server's CA certificate, used by the <c>ON_LOCALHOST_SECURE</c> profile
    /// (printed by Elasticsearch on first start). When empty, any certificate is accepted (local development only).
    /// </summary>
    public string? CertificateFingerprint { get; init; }
}
