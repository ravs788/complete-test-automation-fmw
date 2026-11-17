using Core.Composition;
using Core.Ui;

namespace Core.Mobile.Ui;

/// <summary>Registers the <c>appium</c> UI driver (selected with <c>"MobileUi": "appium"</c> in <c>providers.json</c>).</summary>
public sealed class AppiumUiPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry) =>
        registry.Add<IUiDriverFactory>("appium", () => new AppiumUiDriverFactory());
}
