namespace Core.Logging.Elastic;

/// <summary>
/// Factory for Elasticsearch results publisher. Validates reachability before creating.
/// </summary>
public class ElasticResultsPublisherFactory : IResultsPublisherFactory
{
    public string Name => "elastic";

    public bool TryCreate(LoggingConfig cfg, out IResultsPublisher publisher, out string? reason)
    {
        publisher = null!;
        reason = null;

        var url = ElasticClientFactory.ResolveUrl(ElasticClientFactory.CurrentProfile, cfg);
        var user = cfg.Elastic.Username;
        var pass = cfg.Elastic.Password;

        var probe = ElasticConnectivity.Check(url, user, pass);
        if (!probe.IsUsable)
        {
            reason = probe.Reason;
            return false;
        }

        var client = ElasticClientFactory.Create(ElasticClientFactory.CurrentProfile, cfg);
        publisher = new ElasticResultsPublisher(client);
        return true;
    }
}
