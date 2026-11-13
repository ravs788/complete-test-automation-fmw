namespace Pages.SauceDemo;

using Pages.Common;
using Microsoft.Playwright;
using System.Threading.Tasks;

/// <summary>"Checkout: Overview" step.</summary>
public sealed class CheckoutOverviewPage(IPage page) : BasePage(page)
{
    public ILocator Title => Page.Locator(".title");

    public async Task FinishAsync() => await Page.Locator("#finish").ClickAsync();
}
