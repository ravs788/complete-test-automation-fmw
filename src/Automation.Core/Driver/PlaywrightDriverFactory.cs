namespace Automation.Core.Driver;

using Microsoft.Playwright;
using System.Threading.Tasks;

public static class PlaywrightDriverFactory
{
    /// <summary>
    /// Launches a browser on the given Playwright instance. The caller owns <paramref name="playwright"/>
    /// and must dispose it after closing the browser, otherwise its driver process keeps running.
    /// </summary>
    public static async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright, string browserType = "chromium", bool headless = true)
    {
        return browserType.ToLowerInvariant() switch
        {
            "firefox" => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless }),
            "webkit" => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless }),
            // PLAYWRIGHT_CHANNEL=chrome launches the Google Chrome already installed on the machine
            // (e.g. CI agents), so no Playwright browser download is needed. Unset = bundled Chromium.
            _ => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Channel = Environment.GetEnvironmentVariable("PLAYWRIGHT_CHANNEL") is { Length: > 0 } channel ? channel : null
            })
        };
    }

    public static async Task<IBrowserContext> CreateIsolatedContextAsync(IBrowser browser, BrowserNewContextOptions? options = null)
    {
        return await browser.NewContextAsync(options ?? new BrowserNewContextOptions());
    }
}
