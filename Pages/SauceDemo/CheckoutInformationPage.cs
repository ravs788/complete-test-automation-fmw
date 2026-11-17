using Core.Ui;

namespace Pages.SauceDemo;

public sealed class CheckoutInformationPage(IUiSession session) : SauceDemoPage(session)
{
    public async Task<CheckoutOverviewPage> FillAndContinueAsync(string firstName, string lastName, string postalCode)
    {
        await TestId("firstName").FillAsync(firstName);
        await TestId("lastName").FillAsync(lastName);
        await TestId("postalCode").FillAsync(postalCode);
        await TestId("continue").ClickAsync();
        return new CheckoutOverviewPage(Session);
    }
}
