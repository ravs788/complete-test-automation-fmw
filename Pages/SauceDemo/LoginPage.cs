using Microsoft.Playwright;

namespace Pages.SauceDemo;

public sealed class LoginPage(IPage page) : SauceDemoPage(page)
{
    private ILocator Username => TestId("username");
    private ILocator Password => TestId("password");
    private ILocator LoginButton => TestId("login-button");

    public ILocator ErrorMessage => TestId("error");

    /// <summary>Opens the login page (the context's BaseURL).</summary>
    public async Task OpenAsync() => await GotoAsync("./");

    /// <summary>Signs in and returns the inventory page.</summary>
    public async Task<InventoryPage> LoginAsync(string username, string password)
    {
        await SubmitAsync(username, password);
        return new InventoryPage(Page);
    }

    /// <summary>Submits credentials that should be rejected; stays on the login page (see <see cref="ErrorMessage"/>).</summary>
    public async Task LoginExpectingErrorAsync(string username, string password) => await SubmitAsync(username, password);

    private async Task SubmitAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
