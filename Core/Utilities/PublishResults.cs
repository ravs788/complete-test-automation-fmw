using System;
using System.Globalization;
using Elastic.Clients.Elasticsearch;
using Core.Utilities;

namespace Core.Utilities
{
    /// <summary>
    /// Provides a simple helper to publish <see cref="LogMetadata"/> documents to ElasticSearch,
    /// mirroring the Java helper org.ravs788.extensions.report.PublishResults.
    /// </summary>
    public static class PublishResults
    {
        private static readonly LoggingConfig _loggingCfg = LoggingConfig.Load();

        /// <summary>
        /// Select the elastic profile to use.  If the environment variable
        /// ELASTIC_SERVER is set to one of the enum values, that will be honoured,
        /// otherwise ON_LOCALHOST_INSECURE is used.
        /// </summary>
        private static readonly ElasticServerChoices _serverChoice = Enum.TryParse(
                Environment.GetEnvironmentVariable("ELASTIC_SERVER"),
                ignoreCase: true,
                out ElasticServerChoices parsed)
            ? parsed
            : ElasticServerChoices.ON_LOCALHOST_INSECURE;

        /// <summary>Singleton client instance for the life of the process.</summary>
        private static readonly ElasticsearchClient _client =
            ElasticClientFactory.Create(_serverChoice, _loggingCfg);

        // Determine connectivity once at startup so we can no-op when server is down.
        private static readonly bool _enabled =
            ElasticConnectivity.IsReachable(_loggingCfg.ElasticUrl, _loggingCfg.Username, _loggingCfg.Password);
        private static bool _disabledWarned = false;

        /// <summary>
        /// Indexes the supplied <paramref name="metadata"/> document into the
        /// index search-{ProjectName.ToLowerInvariant()} by default, unless
        /// <paramref name="overrideIndexName"/> is provided.
        /// </summary>
        public static void ToElastic(LogMetadata metadata, string? overrideIndexName = null)
        {
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            if (!_enabled)
            {
                if (!_disabledWarned)
                {
                    System.Console.Error.WriteLine("[PublishResults] Elastic unreachable. Skipping result publish.");
                    _disabledWarned = true;
                }
                return;
            }

            var indexName = overrideIndexName ??
                            $"search-{(metadata.ProjectName ?? "testproject").ToLower(CultureInfo.InvariantCulture)}";

            var response = _client.Index(metadata, idx => idx.Index(indexName));

            if (!response.IsValidResponse)
            {
                throw new InvalidOperationException(
                    $"Failed to index document. ServerError: {response.ElasticsearchServerError?.Error?.Reason}");
            }
        }
    }
}
