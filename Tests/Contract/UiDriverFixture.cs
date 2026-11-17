using Core.Ui;

namespace Tests.Contract;

/// <summary>
/// One browser per contract test class, launched with <typeparamref name="TFactory"/>. The browser follows the
/// <c>BROWSER</c> / <c>HEADLESS</c> environment variables like the Playwright suite (default: chromium, headless).
/// </summary>
public sealed class UiDriverFixture<TFactory> : IAsyncLifetime where TFactory : IUiDriverFactory, new()
{
    private IUiDriver? _driver;

    public async Task InitializeAsync()
    {
        var browser = Environment.GetEnvironmentVariable("BROWSER") is { Length: > 0 } b ? b : "chromium";
        var headless = !string.Equals(Environment.GetEnvironmentVariable("HEADLESS"), "false", StringComparison.OrdinalIgnoreCase);
        _driver = await new TFactory().LaunchAsync(new UiLaunchOptions(browser, headless));
    }

    /// <summary>A fresh session with a short action timeout, so failure checks finish quickly.</summary>
    public Task<IUiSession> NewSessionAsync() =>
        (_driver ?? throw new InvalidOperationException("Browser not launched.")).NewSessionAsync(new UiSessionOptions("about:blank", 2_000));

    public async Task DisposeAsync()
    {
        if (_driver is not null)
        {
            await _driver.DisposeAsync();
        }
    }
}
