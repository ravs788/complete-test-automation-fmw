using System.Runtime.CompilerServices;
using Core.Composition;
using Core.Logging;
using Core.Mobile.Configuration;
using Core.Mobile.Driver;
using Core.Testing;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Tests.UI.Mobile.Infrastructure;

/// <summary>
/// Base for mobile tests. Wrap each test body in <see cref="Run"/>: it starts the Appium session (and opens
/// <see cref="MobileSettings.BaseUrl"/> for Web), attaches a screenshot to the report on failure, publishes the
/// result and quits the driver (via Core's <see cref="TestRunner"/>).
/// </summary>
public abstract class MobileTestBase : IDisposable
{
    protected MobileTestBase()
    {
        Logger = LoggingServiceFactory.CreateLogger("ui-mobile-logs-{0:yyyy.MM.dd}");
    }

    protected MobileSettings Settings => MobileSettingsProvider.Current;
    protected ILoggingService Logger { get; }
    protected AppiumDriver Driver { get; private set; } = null!;

    protected void Run(Action test, [CallerMemberName] string testName = "") =>
        TestRunner.Run(
            new TestRunInfo(Logger, "ui-mobile", GetType(), testName, Browser: "mobile"),
            body: () =>
            {
                StartSession();
                test();
            },
            onFailure: AttachScreenshot,
            cleanup: QuitDriver);

    private void StartSession()
    {
        var (platform, appType) = MobileOptionsBuilder.Parse(Settings);
        Logger.Info($"[Device] {platform} ({appType}) Name={Settings.Device.DeviceName}, PlatformVersion={Settings.Device.PlatformVersion}, UDID={Settings.Device.Udid}");

        Driver = MobileDriverFactory.CreateDriver(Settings, Logger);

        if (appType == MobileAppType.Web && !string.IsNullOrWhiteSpace(Settings.BaseUrl))
        {
            Driver.Navigate().GoToUrl(Settings.BaseUrl);
            Logger.Info($"[SetUp] Navigated to {Settings.BaseUrl} on {platform} {appType}");
        }

        // Show the platform in the report's test name, e.g. "LoginPage_Should_Load (Android Web)".
        Framework.Reporter.UpdateTestName(name => $"{name} ({platform} {appType})");
    }

    private void AttachScreenshot()
    {
        if (Driver is ITakesScreenshot taker)
        {
            var png = taker.GetScreenshot().AsByteArray;
            Framework.Reporter.Attach("Test Failure", "image/png", png, $"failure_{DateTime.Now:yyyyMMddHHmmssfff}.png");
        }
    }

    private void QuitDriver()
    {
        try
        {
            Driver?.Quit();
            Driver?.Dispose();
        }
        catch
        {
            // Best effort: the session may already be gone.
        }
        Driver = null!;
    }

    /// <summary>Safety net in case a test didn't go through <see cref="Run"/>.</summary>
    public void Dispose()
    {
        QuitDriver();
        GC.SuppressFinalize(this);
    }
}
