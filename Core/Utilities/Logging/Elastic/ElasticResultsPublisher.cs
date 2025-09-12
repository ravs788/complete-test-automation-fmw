using System;
using System.Globalization;
using Elastic.Clients.Elasticsearch;

namespace Core.Utilities
{
    /// <summary>
    /// Results publisher that indexes LogMetadata documents into Elasticsearch.
    /// </summary>
    public class ElasticResultsPublisher : IResultsPublisher
    {
        private readonly ElasticsearchClient _client;

        public ElasticResultsPublisher(ElasticsearchClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public void Publish(LogMetadata metadata, string? overrideIndexName = null)
        {
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

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
