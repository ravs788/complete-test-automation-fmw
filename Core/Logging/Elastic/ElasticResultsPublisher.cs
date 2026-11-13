using System.Globalization;
using Elastic.Clients.Elasticsearch;

namespace Core.Logging.Elastic;

/// <summary>
/// Results publisher that indexes LogMetadata documents into Elasticsearch.
/// </summary>
public class ElasticResultsPublisher : IResultsPublisher
{
    private readonly ElasticsearchClient _client;
    private volatile bool _failed;

    public ElasticResultsPublisher(ElasticsearchClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public void Publish(LogMetadata metadata, string? overrideIndexName = null)
    {
        if (metadata is null) throw new ArgumentNullException(nameof(metadata));

        var indexName = overrideIndexName ??
                        $"search-{(metadata.ProjectName ?? "testproject").ToLower(CultureInfo.InvariantCulture)}";

        // After the first failure, stop trying: the error was already reported once (TestRunner logs it), and
        // failing again for every test only adds noise.
        if (_failed)
        {
            return;
        }

        var response = _client.Index(metadata, idx => idx.Index(indexName));

        if (!response.IsValidResponse)
        {
            _failed = true;
            throw new InvalidOperationException(
                $"Failed to index document (results publishing to Elastic is now off for this run). ServerError: {response.ElasticsearchServerError?.Error?.Reason ?? response.ApiCallDetails.HttpStatusCode?.ToString()}");
        }
    }
}
