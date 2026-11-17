using System.Runtime.CompilerServices;
using Core.Composition;
using Core.Logging;
using Core.Mobile.Configuration;
using Core.Mobile.Driver;
using Core.Testing;
using Core.Ui;
using Core.Ui.Capabilities;

namespace Tests.UI.Mobile.Infrastructure;

/// <summary>
/// Base for mobile tests (R47). <see cref="RunAsync"/> opens a <see cref="Session"/> through the mobile UI driver
/// selected in <c>providers.json</c> (<c>"MobileUi": "appium"</c>), opens <see cref="MobileSettings.BaseUrl"/> for
/// Web, attaches a screenshot to the report on failure, publishes the result and ends the session. Tests use the same
/// page objects and <see cref="Ui.Expect"/> assertions as the web suite.
/// </summary>
public abstract class MobileTestBase
{
    /// <summary>Action timeout on the device (slower than a desktop browser).</summary>
    private const float DefaultTimeoutMs = 20_000;

    protected MobileTestBase()
    {
        Logger = LoggingServiceFactory.CreateLogger("ui-mobile-logs-{0:yyyy.MM.dd}");
    }

    protected MobileSettings Settings => MobileSettingsProvider.Current;
    protected ILoggingService Logger { get; }

    /// <summary>This test's device session.</summary>
    protected IUiSession Session { get; private set; } = null!;

    protected Task RunAsync(Func<Task> test, [CallerMemberName] string testName = "") =>
        TestRunner.RunAsync(
            new TestRunInfo(Logger, "ui-mobile", GetType(), testName, Browser: "mobile"),
            body: async () =>
            {
                await StartSessionAsync();
                try
                {
                    await test();
                }
                catch (UiException ex)
                {
                    // Report UI failures as test failures (Allure "failed", not "broken"), like the web suite.
                    Assert.Fail($"{ex.Message}{Environment.NewLine}{ex.InnerException?.StackTrace ?? ex.StackTrace}");
                }
            },
            onFailureAsync: AttachScreenshotAsync,
            cleanupAsync: EndSessionAsync);

    private async Task StartSessionAsync()
    {
        var (platform, appType) = MobileOptionsBuilder.Parse(Settings);
        Logger.Info($"[Device] {platform} ({appType}) Name={Settings.Device.DeviceName}, PlatformVersion={Settings.Device.PlatformVersion}, UDID={Settings.Device.Udid}");

        var driver = await Framework.MobileUiDrivers.LaunchAsync(new UiLaunchOptions(Settings.Android.BrowserName, Headless: false));
        Session = await driver.NewSessionAsync(new UiSessionOptions(Settings.BaseUrl, DefaultTimeoutMs));

        if (appType == MobileAppType.Web && !string.IsNullOrWhiteSpace(Settings.BaseUrl))
        {
            await Session.NavigateAsync(Settings.BaseUrl);
            Logger.Info($"[SetUp] Navigated to {Settings.BaseUrl} on {platform} {appType}");
        }

        // Show the platform in the report's test name, e.g. "LoginPage_Should_Load (Android Web)".
        Framework.Reporter.UpdateTestName(name => $"{name} ({platform} {appType})");
    }

    private async Task AttachScreenshotAsync()
    {
        if (Session != null && Session.TryGet<IScreenshots>(out var screenshots))
        {
            var png = await screenshots.CaptureAsync();
            Framework.Reporter.Attach("Test Failure", "image/png", png, $"failure_{DateTime.Now:yyyyMMddHHmmssfff}.png");
        }
    }

    private async Task EndSessionAsync()
    {
        if (Session != null)
        {
            await Session.DisposeAsync();
            Session = null!;
        }
    }
}
