using OpenQA.Selenium.Appium;
using Tests.UI.Mobile.Configuration;

namespace Tests.UI.Mobile.Driver;

/// <summary>Turns <see cref="MobileSettings"/> into Appium capabilities.</summary>
public static class MobileOptionsBuilder
{
    public static (MobilePlatform platform, MobileAppType appType) Parse(MobileSettings cfg)
    {
        var platform = Enum.TryParse<MobilePlatform>(cfg.Platform, true, out var p) ? p : MobilePlatform.Android;
        var appType = Enum.TryParse<MobileAppType>(cfg.AppType, true, out var t) ? t : MobileAppType.Web;
        return (platform, appType);
    }

    public static AppiumOptions Build(MobileSettings cfg, MobilePlatform platform, MobileAppType appType)
    {
        var opts = new AppiumOptions();
        // Common
        opts.PlatformName = platform.ToString();
        opts.AddAdditionalAppiumOption("newCommandTimeout", cfg.Server.NewCommandTimeoutSec);

        // Device common
        if (!string.IsNullOrWhiteSpace(cfg.Device.DeviceName))
            opts.DeviceName = cfg.Device.DeviceName;
        if (!string.IsNullOrWhiteSpace(cfg.Device.PlatformVersion))
            opts.AddAdditionalAppiumOption("platformVersion", cfg.Device.PlatformVersion);
        if (!string.IsNullOrWhiteSpace(cfg.Device.Udid))
            opts.AddAdditionalAppiumOption("udid", cfg.Device.Udid);

        // Behavior
        opts.AddAdditionalAppiumOption("noReset", cfg.Behavior.NoReset);
        opts.AddAdditionalAppiumOption("fullReset", cfg.Behavior.FullReset);

        if (platform == MobilePlatform.Android)
        {
            opts.AutomationName = string.IsNullOrWhiteSpace(cfg.Android.AutomationName) ? "UiAutomator2" : cfg.Android.AutomationName;

            if (appType == MobileAppType.Native)
            {
                if (!string.IsNullOrWhiteSpace(cfg.Android.App))
                    opts.AddAdditionalAppiumOption("app", cfg.Android.App);
                if (!string.IsNullOrWhiteSpace(cfg.Android.AppPackage))
                    opts.AddAdditionalAppiumOption("appPackage", cfg.Android.AppPackage);
                if (!string.IsNullOrWhiteSpace(cfg.Android.AppActivity))
                    opts.AddAdditionalAppiumOption("appActivity", cfg.Android.AppActivity);
            }
            else
            {
                var browserName = string.IsNullOrWhiteSpace(cfg.Android.BrowserName) ? "Chrome" : cfg.Android.BrowserName;
                if (string.Equals(browserName, "Firefox", StringComparison.OrdinalIgnoreCase))
                {
                    // Firefox on Android via Geckodriver
                    opts.AddAdditionalAppiumOption("browserName", "Firefox");

                    // Optional: direct path to a local geckodriver binary to avoid downloads
                    if (!string.IsNullOrWhiteSpace(cfg.Android.GeckodriverExecutable))
                    {
                        opts.AddAdditionalAppiumOption("geckodriverExecutable", cfg.Android.GeckodriverExecutable);
                    }

                    // moz:firefoxOptions are the canonical way to target Firefox on Android
                    var ffOpts = new Dictionary<string, object>();
                    if (!string.IsNullOrWhiteSpace(cfg.Android.FirefoxPackage))
                    {
                        ffOpts["androidPackage"] = cfg.Android.FirefoxPackage;
                    }
                    if (!string.IsNullOrWhiteSpace(cfg.Android.FirefoxActivity))
                    {
                        ffOpts["androidActivity"] = cfg.Android.FirefoxActivity;
                    }
                    var serial = !string.IsNullOrWhiteSpace(cfg.Device.Udid) ? cfg.Device.Udid : cfg.Device.DeviceName;
                    if (!string.IsNullOrWhiteSpace(serial))
                    {
                        ffOpts["androidDeviceSerial"] = serial;
                    }
                    if (ffOpts.Count > 0)
                    {
                        opts.AddAdditionalAppiumOption("moz:firefoxOptions", ffOpts);
                    }
                }
                else
                {
                    // Chrome/other chromium-based browsers
                    opts.AddAdditionalAppiumOption("browserName", browserName);

                    // Pass options directly to ChromeDriver to suppress first-run/welcome and sync screens
                    opts.AddAdditionalAppiumOption("goog:chromeOptions", new Dictionary<string, object>
                    {
                        ["args"] = new[]
                        {
                            "--no-first-run",
                            "--disable-fre",
                            "--no-default-browser-check",
                            "--disable-popup-blocking",
                            "--disable-notifications",
                            "--disable-features=Translate"
                        }
                    });

                    // If a specific Chromedriver path/dir is provided in config, use it; otherwise fall back to auto-download.
                    if (!string.IsNullOrWhiteSpace(cfg.Android.ChromedriverExecutable))
                    {
                        opts.AddAdditionalAppiumOption("appium:chromedriverExecutable", cfg.Android.ChromedriverExecutable);
                    }
                    if (!string.IsNullOrWhiteSpace(cfg.Android.ChromedriverExecutableDir))
                    {
                        opts.AddAdditionalAppiumOption("appium:chromedriverExecutableDir", cfg.Android.ChromedriverExecutableDir);
                    }
                    if (cfg.Android.ChromedriverArgs != null && cfg.Android.ChromedriverArgs.Count > 0)
                    {
                        // Pass through additional args to chromedriver, e.g. ["--disable-build-check"]
                        opts.AddAdditionalAppiumOption("appium:chromedriverArgs", cfg.Android.ChromedriverArgs);
                    }
                    if (string.IsNullOrWhiteSpace(cfg.Android.ChromedriverExecutable) && string.IsNullOrWhiteSpace(cfg.Android.ChromedriverExecutableDir))
                    {
                        // Help avoid local ChromeDriver version mismatches via auto-download when a path/dir is not supplied
                        opts.AddAdditionalAppiumOption("appium:chromedriverAutodownload", true);
                    }
                }
            }
        }
        else // iOS
        {
            opts.AutomationName = string.IsNullOrWhiteSpace(cfg.IOS.AutomationName) ? "XCUITest" : cfg.IOS.AutomationName;
            opts.AddAdditionalAppiumOption("autoAcceptAlerts", cfg.Behavior.AcceptAlertsAutomatically);

            if (appType == MobileAppType.Native)
            {
                if (!string.IsNullOrWhiteSpace(cfg.IOS.App))
                    opts.AddAdditionalAppiumOption("app", cfg.IOS.App);
                if (!string.IsNullOrWhiteSpace(cfg.IOS.BundleId))
                    opts.AddAdditionalAppiumOption("bundleId", cfg.IOS.BundleId);
            }
            else
            {
                opts.AddAdditionalAppiumOption("browserName", string.IsNullOrWhiteSpace(cfg.IOS.BrowserName) ? "Safari" : cfg.IOS.BrowserName);
            }
        }

        return opts;
    }
}
