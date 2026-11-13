using System;
using Elastic.Clients.Elasticsearch;

namespace Core.Utilities
{
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

            var url = cfg.Elastic?.Url;
            var user = cfg.Elastic?.Username;
            var pass = cfg.Elastic?.Password;

            if (!ElasticConnectivity.IsReachable(url, user, pass))
            {
                reason = $"Elastic unreachable at {url}";
                return false;
            }

            // Build client using existing factory/profile
            var serverChoice = Enum.TryParse(
                    Environment.GetEnvironmentVariable("ELASTIC_SERVER"),
                    ignoreCase: true,
                    out ElasticServerChoices parsed)
                ? parsed
                : ElasticServerChoices.ON_LOCALHOST_INSECURE;

            var client = ElasticClientFactory.Create(serverChoice, cfg);
            publisher = new ElasticResultsPublisher(client);
            return true;
        }
    }
}
