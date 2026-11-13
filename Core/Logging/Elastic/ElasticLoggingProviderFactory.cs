namespace Core.Logging.Elastic;

/// <summary>
/// Provider factory for Elasticsearch-backed ILoggingService.
/// Performs a lightweight reachability probe before creating the service.
/// </summary>
public class ElasticLoggingProviderFactory : ILoggingProviderFactory
{
    public string Name => "elastic";

    public bool TryCreate(LoggingConfig cfg, string indexFormat, out ILoggingService service, out string? reason)
    {
        service = null!;
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

        var elastic = new ElasticLoggingService();
        elastic.Configure(indexFormat);
        service = elastic;
        return true;
    }
}
