namespace Tests.Fixtures;

using Core.Configuration;
using Core.Driver;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using Xunit;

/// <summary>One browser per test class (IClassFixture); each test gets its own isolated context.</summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;

    public IBrowser Browser { get; private set; } = null!;
    public TestSettings Settings => ConfigurationProvider.Current;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        Browser = await PlaywrightDriverFactory.CreateBrowserAsync(
            _playwright,
            browserType: Settings.Browser,
            headless: Settings.Headless
        );
    }

    public async Task<(IBrowserContext Context, IPage Page)> CreateCleanPageAsync(string baseUrl)
    {
        if (Browser == null) throw new InvalidOperationException("Browser not initialized.");

        var context = await PlaywrightDriverFactory.CreateIsolatedContextAsync(Browser, new BrowserNewContextOptions
        {
            BaseURL = baseUrl
        });

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(Settings.DefaultTimeoutMs);

        return (context, page);
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (Browser != null)
            {
                await Browser.CloseAsync();
            }
        }
        finally
        {
            // Stops the Playwright driver (Node) process even if closing the browser failed.
            _playwright?.Dispose();
            _playwright = null;
        }
    }
}
