using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace Core.Ui.Selenium;

/// <summary>
/// Launches browsers with Selenium (<c>selenium</c>): <c>chrome</c> / <c>chromium</c> (installed Chrome),
/// <c>msedge</c> and <c>firefox</c>; Selenium Manager finds or downloads the matching driver. Selenium has no isolated
/// browser contexts, so every session is a new browser.
/// </summary>
public sealed class SeleniumUiDriverFactory : IUiDriverFactory
{
    public Task<IUiDriver> LaunchAsync(UiLaunchOptions options) => Task.FromResult<IUiDriver>(new SeleniumUiDriver(options));
}

public sealed class SeleniumUiDriver(UiLaunchOptions launch) : IUiDriver
{
    public Task<IUiSession> NewSessionAsync(UiSessionOptions options)
    {
        var driver = WebDriverErrors.Run($"start {launch.Browser}", () => Start(launch));
        return Task.FromResult<IUiSession>(new WebDriverUiSession(driver, options));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static IWebDriver Start(UiLaunchOptions launch)
    {
        switch (launch.Browser.ToLowerInvariant())
        {
            case "chrome" or "chromium":
                var chrome = new ChromeOptions();
                chrome.AddArgument("--window-size=1280,800");
                if (launch.Headless)
                {
                    chrome.AddArgument("--headless=new");
                }
                return new ChromeDriver(chrome);
            case "msedge" or "edge":
                var edge = new EdgeOptions();
                edge.AddArgument("--window-size=1280,800");
                if (launch.Headless)
                {
                    edge.AddArgument("--headless=new");
                }
                return new EdgeDriver(edge);
            case "firefox":
                var firefox = new FirefoxOptions();
                if (launch.Headless)
                {
                    firefox.AddArgument("-headless");
                }
                return new FirefoxDriver(firefox);
            default:
                throw new UiException($"Browser '{launch.Browser}' isn't available with the Selenium driver; use chrome, msedge or firefox.");
        }
    }
}
