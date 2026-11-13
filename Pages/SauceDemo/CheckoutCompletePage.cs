using Microsoft.Playwright;

namespace Pages.SauceDemo;

public sealed class CheckoutCompletePage(IPage page) : SauceDemoPage(page)
{
    public ILocator Header => TestId("complete-header");
}
