using Core.Ui;

namespace Pages.SauceDemo;

public sealed class CheckoutCompletePage(IUiSession session) : SauceDemoPage(session)
{
    public IElement Header => TestId("complete-header");
}
