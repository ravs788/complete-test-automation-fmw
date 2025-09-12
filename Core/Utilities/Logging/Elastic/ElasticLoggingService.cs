using System;
using Elastic.Clients.Elasticsearch;

namespace Core.Utilities
{
    /// <summary>
    /// Elasticsearch-backed implementation of <see cref="ILoggingService"/>.
    /// Replaces the previous SerilogLoggingService so that the framework has
    /// a single, unified logging mechanism.
    /// </summary>
    public class ElasticLoggingService : ILoggingService
    {
        private ElasticsearchClient? _client;
        private string _indexFormat = "logs-default";
        private bool _enabled = true;
        private bool _disabledWarned = false;

        /// <inheritdoc/>
        public void Configure(
            string indexFormat,
            string? username = null,
            string? password = null,
            string? elasticUrl = null)
        {
            _indexFormat = string.IsNullOrWhiteSpace(indexFormat)
                ? "logs-default"
                : indexFormat;

            // Prefer values provided in parameters; otherwise fall back to logging-config.json
            var cfg = LoggingConfig.Load();
            cfg.Elastic.Url = !string.IsNullOrWhiteSpace(elasticUrl) ? elasticUrl : cfg.Elastic.Url;
            cfg.Elastic.Username = !string.IsNullOrWhiteSpace(username) ? username : cfg.Elastic.Username;
            cfg.Elastic.Password = !string.IsNullOrWhiteSpace(password) ? password : cfg.Elastic.Password;

            // Use insecure localhost profile by default; user can override via env-var
            var serverChoice = Enum.TryParse(
                    Environment.GetEnvironmentVariable("ELASTIC_SERVER"),
                    ignoreCase: true,
                    out ElasticServerChoices parsed)
                ? parsed
                : ElasticServerChoices.ON_LOCALHOST_INSECURE;

            _client = ElasticClientFactory.Create(serverChoice, cfg);

            // Probe connectivity and disable logging if server is unreachable
            string? probeUrl = cfg.Elastic?.Url;
            if (serverChoice == ElasticServerChoices.ON_LOCALHOST_SECURE && !string.IsNullOrWhiteSpace(probeUrl))
            {
                probeUrl = probeUrl.Replace("http://", "https://");
            }

            _enabled = ElasticConnectivity.IsReachable(probeUrl, cfg.Elastic?.Username, cfg.Elastic?.Password);
            if (!_enabled && !_disabledWarned)
            {
                System.Console.Error.WriteLine($"[ElasticLoggingService] Elastic unreachable at '{probeUrl}'. Logging disabled.");
                _disabledWarned = true;
            }
        }

        /// <inheritdoc/>
        public void Info(string message, LogMetadata? metadata = null) =>
            Write("INFO", message, metadata);

        /// <inheritdoc/>
        public void Error(string message, LogMetadata? metadata = null) =>
            Write("ERROR", message, metadata);

        /// <inheritdoc/>
        public void Debug(string message, LogMetadata? metadata = null) =>
            Write("DEBUG", message, metadata);

        private void Write(string level, string message, LogMetadata? metadata)
        {
            if (_client == null)
                throw new InvalidOperationException("ElasticLoggingService is not configured. Call Configure() first.");

            if (!_enabled)
                return;

            var entry = new ElasticLogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                Metadata = metadata
            };

            try
            {
                var indexName = ResolveIndexName();
                var response = _client.Index(entry, req => req.Index(indexName));
                if (!response.IsValidResponse)
                {
                    System.Console.Error.WriteLine($"[ElasticLoggingService] Indexing failed: {response.ElasticsearchServerError?.Error?.Reason}");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine($"[ElasticLoggingService] Exception while indexing log entry: {ex.Message}");
            }
        }

        private string ResolveIndexName()
        {
            try
            {
                var formatted = string.Format(_indexFormat, DateTime.UtcNow);
                return formatted.ToLowerInvariant();
            }
            catch (System.FormatException)
            {
                return _indexFormat.ToLowerInvariant();
            }
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            _enabled = false;
            // ElasticsearchClient and its transport do not implement IDisposable in the public API.
            // Simply release our reference so it can be GC-collected when no longer used.
            _client = null;
        }

        /// <summary>Document structure stored in Elasticsearch.</summary>
        private class ElasticLogEntry
        {
            public DateTime Timestamp { get; set; }
            public string? Level { get; set; }
            public string? Message { get; set; }
            public LogMetadata? Metadata { get; set; }
        }
    }
}
