using Microsoft.Playwright;

namespace Core.Ui.Playwright;

/// <summary>Launches browsers with Playwright (<c>playwright</c>): chromium, chrome, msedge, firefox or webkit.</summary>
public sealed class PlaywrightUiDriverFactory : IUiDriverFactory
{
    public async Task<IUiDriver> LaunchAsync(UiLaunchOptions options)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        try
        {
            var browser = await PlaywrightDriverFactory.CreateBrowserAsync(playwright, options.Browser, options.Headless);
            return new PlaywrightUiDriver(playwright, browser);
        }
        catch
        {
            playwright.Dispose();
            throw;
        }
    }
}

/// <summary>One launched Playwright browser; each session is a new isolated browser context with one page.</summary>
public sealed class PlaywrightUiDriver(IPlaywright playwright, IBrowser browser) : IUiDriver
{
    public async Task<IUiSession> NewSessionAsync(UiSessionOptions options)
    {
        var context = await PlaywrightDriverFactory.CreateIsolatedContextAsync(browser, new BrowserNewContextOptions { BaseURL = options.BaseUrl });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(options.DefaultTimeoutMs);
        return new PlaywrightUiSession(context, page);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await browser.CloseAsync();
        }
        finally
        {
            // Stops the Playwright driver (Node) process even if closing the browser failed.
            playwright.Dispose();
        }
    }
}
