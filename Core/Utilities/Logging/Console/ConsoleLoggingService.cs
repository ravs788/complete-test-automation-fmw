using System;
using System.Text.Json;

namespace Core.Utilities
{
    /// <summary>
    /// Console-backed implementation of ILoggingService.
    /// Intended as a lightweight fallback when Elasticsearch is unavailable.
    /// </summary>
    public class ConsoleLoggingService : ILoggingService
    {
        private string _indexFormat = "logs-default";

        public void Configure(string indexFormat, string? username = null, string? password = null, string? elasticUrl = null)
        {
            _indexFormat = string.IsNullOrWhiteSpace(indexFormat) ? "logs-default" : indexFormat;
        }

        public void Info(string message, LogMetadata? metadata = null) =>
            Write("INFO", message, metadata, isError: false);

        public void Error(string message, LogMetadata? metadata = null) =>
            Write("ERROR", message, metadata, isError: true);

        public void Debug(string message, LogMetadata? metadata = null) =>
            Write("DEBUG", message, metadata, isError: false);

        private void Write(string level, string message, LogMetadata? metadata, bool isError)
        {
            var ts = DateTime.UtcNow.ToString("o");
            var indexName = ResolveIndexName();
            string line = $"[{ts}] [{level}] [{indexName}] {message}";

            if (metadata != null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = false });
                    line += $" | metadata={json}";
                }
                catch
                {
                    // ignore metadata serialization errors for console logging
                }
            }

            if (isError)
            {
                System.Console.Error.WriteLine(line);
            }
            else
            {
                System.Console.WriteLine(line);
            }
        }

        private string ResolveIndexName()
        {
            try
            {
                var formatted = string.Format(_indexFormat, DateTime.UtcNow);
                return formatted.ToLowerInvariant();
            }
            catch (FormatException)
            {
                return _indexFormat.ToLowerInvariant();
            }
        }

        public void Shutdown()
        {
            // no resources to dispose
        }
    }
}
