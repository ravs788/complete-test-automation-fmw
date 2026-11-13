using Microsoft.Playwright;

namespace Pages.SauceDemo;

public sealed class CheckoutInformationPage(IPage page) : SauceDemoPage(page)
{
    public async Task FillAndContinueAsync(string firstName, string lastName, string postalCode)
    {
        await TestId("firstName").FillAsync(firstName);
        await TestId("lastName").FillAsync(lastName);
        await TestId("postalCode").FillAsync(postalCode);
        await TestId("continue").ClickAsync();
    }
}
