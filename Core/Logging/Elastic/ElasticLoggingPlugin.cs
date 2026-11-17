using Core.Composition;

namespace Core.Logging.Elastic;

/// <summary>Registers the <c>elastic</c> logging and results-publishing providers (select them in <c>providers.json</c>).</summary>
public sealed class ElasticLoggingPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry)
    {
        registry.Add<ILoggingProviderFactory>("elastic", () => new ElasticLoggingProviderFactory());
        registry.Add<IResultsPublisherFactory>("elastic", () => new ElasticResultsPublisherFactory());
    }
}
