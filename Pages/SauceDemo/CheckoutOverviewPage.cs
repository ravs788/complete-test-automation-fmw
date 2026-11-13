using Microsoft.Playwright;

namespace Pages.SauceDemo;

public sealed class CheckoutOverviewPage(IPage page) : SauceDemoPage(page)
{
    public async Task FinishAsync() => await TestId("finish").ClickAsync();
}
