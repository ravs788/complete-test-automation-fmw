using Elastic.Clients.Elasticsearch;

namespace Core.Logging.Elastic;

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

    // Where log lines go once Elastic can't be used, so nothing is lost.
    private readonly Console.ConsoleLoggingService _fallback = new();

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
        _fallback.Configure(_indexFormat);

        // Prefer values provided in parameters; otherwise fall back to logging-config.json
        var cfg = LoggingConfigProvider.Current;
        cfg = cfg with
        {
            Elastic = cfg.Elastic with
            {
                Url = !string.IsNullOrWhiteSpace(elasticUrl) ? elasticUrl : cfg.Elastic.Url,
                Username = !string.IsNullOrWhiteSpace(username) ? username : cfg.Elastic.Username,
                Password = !string.IsNullOrWhiteSpace(password) ? password : cfg.Elastic.Password
            }
        };

        var serverChoice = ElasticClientFactory.CurrentProfile;
        _client = ElasticClientFactory.Create(serverChoice, cfg);

        // Disable logging if the server can't be used (the probe result is cached per URL for the run).
        var probe = ElasticConnectivity.Check(ElasticClientFactory.ResolveUrl(serverChoice, cfg), cfg.Elastic.Username, cfg.Elastic.Password);
        _enabled = probe.IsUsable;
        if (!_enabled)
        {
            Disable(probe.Reason);
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
        {
            WriteToFallback(level, message, metadata);
            return;
        }

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
                Disable($"indexing failed: {response.ElasticsearchServerError?.Error?.Reason ?? response.ApiCallDetails.OriginalException?.Message ?? response.ApiCallDetails.HttpStatusCode?.ToString() ?? "no response"}");
                WriteToFallback(level, message, metadata);
            }
        }
        catch (Exception ex)
        {
            Disable($"exception while indexing: {ex.Message}");
            WriteToFallback(level, message, metadata);
        }
    }

    private void WriteToFallback(string level, string message, LogMetadata? metadata)
    {
        switch (level)
        {
            case "ERROR": _fallback.Error(message, metadata); break;
            case "DEBUG": _fallback.Debug(message, metadata); break;
            default: _fallback.Info(message, metadata); break;
        }
    }

    /// <summary>Stops writing to Elastic after the first failure and says why, once, instead of failing on every line.</summary>
    private void Disable(string? reason)
    {
        _enabled = false;
        if (!_disabledWarned)
        {
            _disabledWarned = true;
            System.Console.Error.WriteLine($"[ElasticLoggingService] Elastic logging disabled for '{_indexFormat}', logging to the console instead: {reason}");
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
