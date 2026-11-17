using Core.Composition;

namespace Core.Resilience.Polly;

/// <summary>Registers the <c>polly</c> retry policies (select them with <c>"Resilience": "polly"</c> in <c>providers.json</c>).</summary>
public sealed class PollyResiliencePlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry) =>
        registry.Add<IRetryPolicyFactory>("polly", () => new PollyRetryPolicyFactory());
}
