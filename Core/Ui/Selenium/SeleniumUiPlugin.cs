using Core.Composition;

namespace Core.Ui.Selenium;

/// <summary>Registers the <c>selenium</c> UI driver (select it with <c>"Ui": "selenium"</c> in <c>providers.json</c>).</summary>
public sealed class SeleniumUiPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry) =>
        registry.Add<IUiDriverFactory>("selenium", () => new SeleniumUiDriverFactory());
}
