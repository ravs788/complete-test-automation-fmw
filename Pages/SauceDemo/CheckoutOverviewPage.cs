using Microsoft.Playwright;

namespace Pages.SauceDemo;

public sealed class CheckoutOverviewPage(IPage page) : SauceDemoPage(page)
{
    public async Task<CheckoutCompletePage> FinishAsync()
    {
        await TestId("finish").ClickAsync();
        return new CheckoutCompletePage(Page);
    }
}
