using Microsoft.Playwright;

namespace Core.Driver;

public static class PlaywrightDriverFactory
{
    /// <summary>
    /// Launches a browser on the given Playwright instance. The caller owns <paramref name="playwright"/>
    /// and must dispose it after closing the browser, otherwise its driver process keeps running.
    /// </summary>
    /// <param name="browserType">
    /// chromium (Playwright's bundled build), chrome / msedge (the installed Google Chrome / Microsoft Edge,
    /// no download needed), firefox, or webkit.
    /// </param>
    public static async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright, string browserType = "chromium", bool headless = true)
    {
        return browserType.ToLowerInvariant() switch
        {
            "firefox" => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless }),
            "webkit" => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless }),
            "chrome" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless, Channel = "chrome" }),
            "msedge" or "edge" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless, Channel = "msedge" }),
            "chromium" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless }),
            _ => throw new ArgumentException($"Unknown browser '{browserType}'. Use chromium, chrome, msedge, firefox or webkit.", nameof(browserType))
        };
    }

    public static async Task<IBrowserContext> CreateIsolatedContextAsync(IBrowser browser, BrowserNewContextOptions? options = null)
    {
        return await browser.NewContextAsync(options ?? new BrowserNewContextOptions());
    }
}
