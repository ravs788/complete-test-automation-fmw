using Microsoft.Playwright;
using Pages.Common;

namespace Pages.SauceDemo;
/// <summary>"Checkout: Your Information" step.</summary>
public sealed class CheckoutInformationPage(IPage page) : BasePage(page)
{
    public ILocator Title => Page.Locator(".title");

    public async Task FillAndContinueAsync(string firstName, string lastName, string postalCode)
    {
        await Page.Locator("#first-name").FillAsync(firstName);
        await Page.Locator("#last-name").FillAsync(lastName);
        await Page.Locator("#postal-code").FillAsync(postalCode);
        await Page.Locator("#continue").ClickAsync();
    }
}
