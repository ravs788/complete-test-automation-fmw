namespace Pages.SauceDemo;

using Pages.Common;
using Microsoft.Playwright;

public sealed class CheckoutCompletePage(IPage page) : BasePage(page)
{
    public ILocator Header => Page.Locator(".complete-header");
}
