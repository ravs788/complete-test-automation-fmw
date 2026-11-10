namespace Automation.Tests.Fixtures;

using Automation.Core.Configuration;
using Automation.Core.Driver;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using Xunit;

public class PlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; private set; } = null!;
    public TestSettings Settings => ConfigurationProvider.Current;

    public async Task InitializeAsync()
    {
        Browser = await PlaywrightDriverFactory.CreateBrowserAsync(
            browserType: Settings.Browser,
            headless: Settings.Headless
        );
    }

    public async Task<(IBrowserContext Context, IPage Page)> CreateCleanPageAsync()
    {
        if (Browser == null) throw new InvalidOperationException("Browser not initialized.");

        var context = await PlaywrightDriverFactory.CreateIsolatedContextAsync(Browser, new BrowserNewContextOptions
        {
            BaseURL = Settings.BaseUrl
        });

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(Settings.DefaultTimeoutMs);

        return (context, page);
    }

    public async Task DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
    }
}
