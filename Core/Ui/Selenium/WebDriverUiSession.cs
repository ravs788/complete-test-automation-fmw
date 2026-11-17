using Core.Ui.Capabilities;
using OpenQA.Selenium;

namespace Core.Ui.Selenium;

/// <summary>
/// <see cref="IUiSession"/> on any W3C WebDriver (Selenium browsers, Appium). Supports screenshots (the visible
/// viewport); tracing isn't available through WebDriver. Quits the driver when disposed.
/// </summary>
public class WebDriverUiSession : IUiSession, IScreenshots
{
    private readonly Uri? _baseUri;

    public WebDriverUiSession(IWebDriver driver, UiSessionOptions options)
    {
        Driver = driver;
        Timeout = TimeSpan.FromMilliseconds(options.DefaultTimeoutMs);
        _baseUri = Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri) ? baseUri : null;
    }

    protected IWebDriver Driver { get; }

    /// <summary>How long actions wait for an element.</summary>
    protected TimeSpan Timeout { get; }

    public Task NavigateAsync(string url)
    {
        var target = Uri.TryCreate(url, UriKind.Absolute, out var absolute) ? absolute
            : _baseUri is not null ? new Uri(_baseUri, url)
            : throw new UiException($"Could not navigate to {url}: it is relative and the session has no base URL.");
        WebDriverErrors.Run($"navigate to {target}", () => Driver.Navigate().GoToUrl(target));
        return Task.CompletedTask;
    }

    public Task<string> TitleAsync() => Task.FromResult(WebDriverErrors.Run("read the title", () => Driver.Title ?? ""));

    public IElement Find(Locate locate) =>
        new WebDriverElement(() => WebDriverLocate.Query(Driver, locate), locate.ToString(), Timeout);

    public bool TryGet<TCapability>(out TCapability capability) where TCapability : class
    {
        capability = (this as TCapability)!;
        return capability is not null;
    }

    public T Native<T>() where T : class =>
        Driver as T ?? throw new NotSupportedException($"The WebDriver session has no {typeof(T).Name}; use IWebDriver.");

    /// <summary>The visible viewport (WebDriver has no full-page screenshot); <paramref name="fullPage"/> is ignored.</summary>
    public Task<byte[]> CaptureAsync(bool fullPage = true) =>
        Task.FromResult(WebDriverErrors.Run("take a screenshot", () => ((ITakesScreenshot)Driver).GetScreenshot().AsByteArray));

    public virtual ValueTask DisposeAsync()
    {
        try
        {
            Driver.Quit();
        }
        catch (WebDriverException)
        {
            // Best effort: the session may already be gone.
        }
        Driver.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
