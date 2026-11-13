namespace Automation.Core.Driver;

using Microsoft.Playwright;
using System.Threading.Tasks;

public static class PlaywrightDriverFactory
{
    public static async Task<IBrowser> CreateBrowserAsync(string browserType = "chromium", bool headless = true)
    {
        var playwright = await Playwright.CreateAsync();
        return browserType.ToLowerInvariant() switch
        {
            "firefox" => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless }),
            "webkit" => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless }),
            _ => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless })
        };
    }

    public static async Task<IBrowserContext> CreateIsolatedContextAsync(IBrowser browser, BrowserNewContextOptions? options = null)
    {
        return await browser.NewContextAsync(options ?? new BrowserNewContextOptions());
    }
}
