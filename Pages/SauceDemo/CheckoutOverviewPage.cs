using Microsoft.Playwright;
using Pages.Common;

namespace Pages.SauceDemo;
/// <summary>"Checkout: Overview" step.</summary>
public sealed class CheckoutOverviewPage(IPage page) : BasePage(page)
{
    public ILocator Title => Page.Locator(".title");

    public async Task FinishAsync() => await Page.Locator("#finish").ClickAsync();
}
