using Core.Composition;

namespace Core.Ui.Playwright;

/// <summary>Registers the <c>playwright</c> UI driver (select it with <c>"Ui": "playwright"</c> in <c>providers.json</c>).</summary>
public sealed class PlaywrightUiPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry) =>
        registry.Add<IUiDriverFactory>("playwright", () => new PlaywrightUiDriverFactory());
}
