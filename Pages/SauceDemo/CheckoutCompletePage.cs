using Microsoft.Playwright;
using Pages.Common;

namespace Pages.SauceDemo;

public sealed class CheckoutCompletePage(IPage page) : BasePage(page)
{
    public ILocator Header => Page.Locator(".complete-header");
}
