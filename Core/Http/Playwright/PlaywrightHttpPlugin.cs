using Core.Composition;

namespace Core.Http.Playwright;

/// <summary>Registers the <c>playwright</c> HTTP transport (select it with <c>"Http": "playwright"</c> in <c>providers.json</c>).</summary>
public sealed class PlaywrightHttpPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry) =>
        registry.Add<IHttpTransportFactory>("playwright", () => new PlaywrightHttpTransportFactory());
}
