using Core.Composition;
using Core.Configuration;
using Core.Ui;

namespace Tests.Fixtures;

/// <summary>
/// One browser per test class, launched by the UI driver selected with <c>"Ui"</c> in <c>providers.json</c> (R42);
/// each test gets its own isolated session from <see cref="NewSessionAsync"/>.
/// </summary>
public class BrowserFixture : IAsyncLifetime
{
    private IUiDriver? _driver;

    public TestSettings Settings => ConfigurationProvider.Current;

    public async Task InitializeAsync() =>
        _driver = await Framework.UiDrivers.LaunchAsync(new UiLaunchOptions(Settings.Browser, Settings.Headless));

    /// <summary>A fresh session (new cookies and storage, one page) for <paramref name="baseUrl"/>.</summary>
    public Task<IUiSession> NewSessionAsync(string baseUrl, float? defaultTimeoutMs = null) =>
        (_driver ?? throw new InvalidOperationException("Browser not launched."))
            .NewSessionAsync(new UiSessionOptions(baseUrl, defaultTimeoutMs ?? Settings.DefaultTimeoutMs));

    public async Task DisposeAsync()
    {
        if (_driver is not null)
        {
            await _driver.DisposeAsync();
            _driver = null;
        }
    }
}
