using System.Net;
using Core.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Remote;
using Tests.UI.Mobile.Configuration;

namespace Tests.UI.Mobile.Driver;

/// <summary>Creates an Android or iOS Appium session from <see cref="MobileSettings"/>.</summary>
public static class MobileDriverFactory
{
    public static AppiumDriver CreateDriver(MobileSettings config, ILoggingService logger)
    {
        var (platform, appType) = MobileOptionsBuilder.Parse(config);
        var options = MobileOptionsBuilder.Build(config, platform, appType);
        var serverUri = new Uri(config.Server.Url);
        var initTimeout = TimeSpan.FromSeconds(config.Server.CommandTimeoutSec <= 0 ? 120 : config.Server.CommandTimeoutSec);

        var executor = new HttpCommandExecutor(serverUri, initTimeout);
        if (serverUri.IsLoopback)
        {
            executor.Proxy = new WebProxy();
        }

        AppiumDriver driver;
        try
        {
            driver = platform switch
            {
                MobilePlatform.Android => new AndroidDriver(executor, options),
                MobilePlatform.iOS => new IOSDriver(executor, options),
                _ => new AndroidDriver(executor, options)
            };
        }
        catch
        {
            executor.Dispose();
            throw;
        }

        logger.Info($"[Driver] Created {platform} {appType} session. SessionId={driver.SessionId}");

        // Orientation is a best-effort hint: some devices and browsers don't support changing it.
        try
        {
            driver.Orientation = string.Equals(config.Device.Orientation, "landscape", StringComparison.OrdinalIgnoreCase)
                ? ScreenOrientation.Landscape
                : ScreenOrientation.Portrait;
        }
        catch (WebDriverException ex)
        {
            logger.Debug($"[Driver] Orientation not set: {ex.Message}");
        }

        return driver;
    }
}
