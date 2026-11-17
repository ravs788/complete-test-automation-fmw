using Core.Ui;

namespace Pages.SauceDemo;

public sealed class CheckoutOverviewPage(IUiSession session) : SauceDemoPage(session)
{
    public async Task<CheckoutCompletePage> FinishAsync()
    {
        await TestId("finish").ClickAsync();
        return new CheckoutCompletePage(Session);
    }
}
