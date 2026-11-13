using Microsoft.Playwright;
using Pages.Common;

namespace Pages.SauceDemo;

public sealed class LoginPage(IPage page) : BasePage(page)
{
    private ILocator Username => Page.Locator("#user-name");
    private ILocator Password => Page.Locator("#password");
    private ILocator LoginButton => Page.Locator("#login-button");

    public ILocator ErrorMessage => Page.Locator("[data-test='error']");

    /// <summary>Opens the login page (the context's BaseURL).</summary>
    public async Task OpenAsync() => await Page.GotoAsync("./");

    public async Task LoginAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
