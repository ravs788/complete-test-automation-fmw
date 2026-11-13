using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Core.Logging.Elastic;

/// <summary>
/// Builds an <see cref="ElasticsearchClient"/> for an <see cref="ElasticServerChoices"/> profile from the
/// <c>Elastic</c> section of <see cref="LoggingConfig"/>. Mirrors the Java class
/// org.ravs788.extensions.report.ElasticLowLevelRestClientFactory.
/// </summary>
public static class ElasticClientFactory
{
    /// <summary>The profile from the <c>ELASTIC_SERVER</c> environment variable; defaults to <c>ON_LOCALHOST_INSECURE</c>.</summary>
    public static ElasticServerChoices CurrentProfile =>
        Enum.TryParse(Environment.GetEnvironmentVariable("ELASTIC_SERVER"), ignoreCase: true, out ElasticServerChoices parsed)
            ? parsed
            : ElasticServerChoices.ON_LOCALHOST_INSECURE;

    /// <summary>The URL the profile connects to (the secure local profile switches to https).</summary>
    public static string? ResolveUrl(ElasticServerChoices serverChoice, LoggingConfig config) =>
        serverChoice == ElasticServerChoices.ON_LOCALHOST_SECURE
            ? (config.Elastic.Url ?? "https://localhost:9200").Replace("http://", "https://")
            : config.Elastic.Url;

    /// <summary>
    /// Creates a configured <see cref="ElasticsearchClient"/>. The client is thread-safe and meant to be reused.
    /// </summary>
    public static ElasticsearchClient Create(ElasticServerChoices serverChoice, LoggingConfig config)
    {
        var elastic = config.Elastic;
        var settings = serverChoice switch
        {
            ElasticServerChoices.ON_CLOUD => new ElasticsearchClientSettings(new Uri(
                elastic.Url ?? throw new InvalidOperationException("Elastic.Url (or ELASTIC_URL) is required for ON_CLOUD."))),
            ElasticServerChoices.ON_LOCALHOST_SECURE => WithCertificateCheck(
                new ElasticsearchClientSettings(new Uri(ResolveUrl(serverChoice, config)!)), elastic.CertificateFingerprint),
            ElasticServerChoices.ON_LOCALHOST_INSECURE => new ElasticsearchClientSettings(new Uri(elastic.Url ?? "http://localhost:9200")),
            _ => throw new ArgumentOutOfRangeException(nameof(serverChoice), serverChoice, null)
        };

        if (!string.IsNullOrEmpty(elastic.Username))
        {
            settings = settings.Authentication(new BasicAuthentication(elastic.Username, elastic.Password ?? string.Empty));
        }

        return new ElasticsearchClient(settings);
    }

    private static ElasticsearchClientSettings WithCertificateCheck(ElasticsearchClientSettings settings, string? fingerprint) =>
        string.IsNullOrWhiteSpace(fingerprint)
            // No fingerprint configured: accept the local self-signed certificate (development only).
            ? settings.ServerCertificateValidationCallback((_, _, _, _) => true)
            : settings.CertificateFingerprint(fingerprint);
}
