using System;
using System.Net.Http;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Core.Utilities;

namespace Core.Utilities
{
    /// <summary>
    /// Factory that produces a ready-to-use <see cref="ElasticsearchClient"/> based on the
    /// ElasticServerChoices profile and credentials defined in logging-config.json.
    /// Mirrors the behaviour of the Java class
    /// org.ravs788.extensions.report.ElasticLowLevelRestClientFactory.
    /// </summary>
    public static class ElasticClientFactory
    {
        /// <summary>
        /// Creates a configured <see cref="ElasticsearchClient" />.  The client instance is lightweight and
        /// intended to be reused across the lifetime of the test run.
        /// </summary>
        public static ElasticsearchClient Create(ElasticServerChoices serverChoice, LoggingConfig config)
        {
            return serverChoice switch
            {
                ElasticServerChoices.ON_CLOUD => CreateCloudClient(config),
                ElasticServerChoices.ON_LOCALHOST_SECURE => CreateLocalHttpsClient(config),
                ElasticServerChoices.ON_LOCALHOST_INSECURE => CreateLocalHttpClient(config),
                _ => throw new ArgumentOutOfRangeException(nameof(serverChoice), serverChoice, null)
            };
        }

        private static ElasticsearchClient CreateLocalHttpClient(LoggingConfig cfg)
        {
            var uri = new Uri(cfg.ElasticUrl ?? "http://localhost:9200");
            var settings = new ElasticsearchClientSettings(uri);

            if (!string.IsNullOrEmpty(cfg.Username) && !string.IsNullOrEmpty(cfg.Password))
            {
                settings = settings.Authentication(new BasicAuthentication(cfg.Username, cfg.Password));
            }

            return new ElasticsearchClient(settings);
        }

        // NOTE: For brevity, HTTPS/Cloud implementations reuse the same basic pattern.
        // Extend with SSL pinning / API-Key auth as needed.

        private static ElasticsearchClient CreateLocalHttpsClient(LoggingConfig cfg)
        {
            var uri = new Uri(cfg.ElasticUrl?.Replace("http://", "https://") ?? "https://localhost:9200");
            var settings = new ElasticsearchClientSettings(uri)
                               .CertificateFingerprint(cfg.Password) // repurpose Password for fingerprint if supplied
                               .ServerCertificateValidationCallback((_, _, _, _) => true);

            if (!string.IsNullOrEmpty(cfg.Username))
                settings = settings.Authentication(new BasicAuthentication(cfg.Username, cfg.Password));

            return new ElasticsearchClient(settings);
        }

        private static ElasticsearchClient CreateCloudClient(LoggingConfig cfg)
        {
            // For Elastic Cloud, URL should already be https://cluster-id.region.elastic-cloud.com
            var uri = new Uri(cfg.ElasticUrl ?? throw new ArgumentNullException(nameof(cfg.ElasticUrl)));
            var settings = new ElasticsearchClientSettings(uri);

            if (!string.IsNullOrEmpty(cfg.Username))
            {
                settings = settings.Authentication(new BasicAuthentication(cfg.Username, cfg.Password));
            }

            return new ElasticsearchClient(settings);
        }
    }
}
