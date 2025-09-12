using System;

namespace Core.Utilities
{
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

            var url = cfg.Elastic?.Url;
            var user = cfg.Elastic?.Username;
            var pass = cfg.Elastic?.Password;

            if (!ElasticConnectivity.IsReachable(url, user, pass))
            {
                reason = $"Elastic unreachable at {url}";
                return false;
            }

            var elastic = new ElasticLoggingService();
            elastic.Configure(indexFormat, user, pass, url);
            service = elastic;
            return true;
        }
    }
}
