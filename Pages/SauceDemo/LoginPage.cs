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

    public async Task LoginAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}
