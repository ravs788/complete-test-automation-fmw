using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;

namespace Core.Utilities
{
    /// <summary>
    /// Results publisher that indexes test result fields into Elasticsearch/OpenSearch
    /// using a raw HTTP POST for maximum compatibility.
    /// Produces a flattened document with top-level fields (no nested metadata).
    /// </summary>
    public class ElasticResultsPublisher : IResultsPublisher
    {
        // Keep the constructor signature for factory compatibility, but do not use the typed client
        private readonly ElasticsearchClient _client;

        public ElasticResultsPublisher(ElasticsearchClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public void Publish(LogMetadata metadata, string? overrideIndexName = null)
        {
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            var cfg = LoggingConfig.Load();
            var baseUrl = (cfg.Elastic?.Url ?? "http://localhost:9200").TrimEnd('/');
            var indexName = overrideIndexName ??
                            $"search-{(metadata.ProjectName ?? "testproject").ToLowerInvariant()}";

            // Flattened payload at top level (no nested metadata)
            var entry = new
            {
                timestamp = DateTime.UtcNow,
                // Level/Message are optional for results; omit to keep payload concise
                projectname = metadata.ProjectName,
                testclassname = metadata.TestClassName,
                testmethodname = metadata.TestMethodName,
                status = metadata.Status,
                duration = metadata.Duration,
                failurereason = metadata.Reason,
                runtime = metadata.RunTime,
                runname = metadata.RunName,
                triggeredby = metadata.TriggeredBy,
                browser = metadata.Browser,
                starttime = metadata.StartTime,
                endtime = metadata.EndTime
            };

            using var http = new HttpClient();
            // Basic auth if provided
            if (!string.IsNullOrEmpty(cfg.Elastic?.Username))
            {
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{cfg.Elastic.Username}:{cfg.Elastic.Password ?? string.Empty}"));
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }

            var json = JsonSerializer.Serialize(entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{baseUrl}/{indexName}/_doc";

            try
            {
                var resp = http.PostAsync(url, content).Result;
                if (!resp.IsSuccessStatusCode)
                {
                    var body = resp.Content.ReadAsStringAsync().Result;
                    throw new InvalidOperationException(
                        $"Failed to index document via HTTP. Status={(int)resp.StatusCode} {resp.ReasonPhrase}. Body={body}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to index document via HTTP: {ex.Message}", ex);
            }
        }
    }
}
