using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        private string? _baseUrl;
        private string? _authToken;
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
            _baseUrl = cfg.Elastic?.Url?.TrimEnd('/');

            if (!string.IsNullOrEmpty(cfg.Elastic?.Username) && !string.IsNullOrEmpty(cfg.Elastic?.Password))
            {
                _authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{cfg.Elastic.Username}:{cfg.Elastic.Password}"));
            }

            // Probe connectivity and disable logging if server is unreachable
            string? probeUrl = _baseUrl;
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
                timestamp = DateTime.UtcNow,
                level = level,
                message = message,
                projectname = metadata?.ProjectName,
                testclassname = metadata?.TestClassName,
                testmethodname = metadata?.TestMethodName,
                status = metadata?.Status,
                duration = metadata?.Duration,
                failurereason = metadata?.Reason,
                runtime = metadata?.RunTime,
                runname = metadata?.RunName,
                triggeredby = metadata?.TriggeredBy,
                browser = metadata?.Browser,
                starttime = metadata?.StartTime,
                endtime = metadata?.EndTime
            };

            var indexName = ResolveIndexName();

            try
            {
                // Use raw HTTP for all logs to ensure compatibility with OpenSearch and avoid content-type issues
                SendViaHttp(entry, indexName);
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine($"[ElasticLoggingService] Exception while indexing log entry: {ex.Message}");
                // Best-effort retry already uses HTTP; nothing else to do here
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
            public DateTime timestamp { get; set; }
            public string? level { get; set; }
            public string? message { get; set; }
            public string? projectname { get; set; }
            public string? testclassname { get; set; }
            public string? testmethodname { get; set; }
            public string? status { get; set; }
            public string? duration { get; set; }
            public string? failurereason { get; set; }
            public string? runtime { get; set; }
            public string? runname { get; set; }
            public string? triggeredby { get; set; }
            public string? browser { get; set; }
            public DateTime? starttime { get; set; }
            public DateTime? endtime { get; set; }
        }

        private void SendViaHttp(ElasticLogEntry entry, string indexName)
        {
            if (string.IsNullOrEmpty(_baseUrl))
                return;

            using var http = new HttpClient();
            if (!string.IsNullOrEmpty(_authToken))
            {
                http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authToken);
            }

            var json = JsonSerializer.Serialize(entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{_baseUrl}/{indexName}/_doc";

            try
            {
                var resp = http.PostAsync(url, content).Result;
                if (!resp.IsSuccessStatusCode)
                {
                    System.Console.Error.WriteLine($"[ElasticLoggingService] HTTP indexing failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine($"[ElasticLoggingService] HTTP indexing exception: {ex.Message}");
            }
        }
    }
}
