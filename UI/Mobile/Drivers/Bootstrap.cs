using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using UI.Mobile.Utilities;

namespace UI.Mobile.Drivers
{
    public enum MobilePlatform
    {
        Android,
        iOS
    }

    public enum MobileAppType
    {
        Native,
        Web
    }

    public interface IMobileDriverFactory
    {
        AppiumDriver CreateDriver(ConfigSettings config, ILoggingService logger);
    }

    public static class MobileOptionsBuilder
    {
        public static (MobilePlatform platform, MobileAppType appType) Parse(ConfigSettings cfg)
        {
            var platform = Enum.TryParse<MobilePlatform>(cfg.Platform, true, out var p) ? p : MobilePlatform.Android;
            var appType = Enum.TryParse<MobileAppType>(cfg.AppType, true, out var t) ? t : MobileAppType.Web;
            return (platform, appType);
        }

        public static AppiumOptions Build(ConfigSettings cfg, MobilePlatform platform, MobileAppType appType)
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

    public class MobileDriverFactory : IMobileDriverFactory
    {
        public AppiumDriver CreateDriver(ConfigSettings config, ILoggingService logger)
        {
            DisableProxyForLocalAppium();

            var (platform, appType) = MobileOptionsBuilder.Parse(config);
            var options = MobileOptionsBuilder.Build(config, platform, appType);
            var serverUri = new Uri(config.Server.Url);
            var initTimeout = TimeSpan.FromSeconds(config.Server.CommandTimeoutSec <= 0 ? 120 : config.Server.CommandTimeoutSec);

            AppiumDriver driver = platform switch
            {
                MobilePlatform.Android => new AndroidDriver(serverUri, options, initTimeout),
                MobilePlatform.iOS => new IOSDriver(serverUri, options, initTimeout),
                _ => new AndroidDriver(serverUri, options, initTimeout)
            };

            try
            {
                logger.Info($"[Driver] Created {platform} {appType} session. SessionId={driver?.SessionId}");
                // Orientation hint (best-effort)
                if (string.Equals(config.Device.Orientation, "landscape", StringComparison.OrdinalIgnoreCase))
                {
                    try { driver.Orientation = ScreenOrientation.Landscape; } catch { }
                }
                else
                {
                    try { driver.Orientation = ScreenOrientation.Portrait; } catch { }
                }
            }
            catch { }

            return driver;
        }

        private static void DisableProxyForLocalAppium()
        {
            foreach (var variableName in new[] { "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "http_proxy", "https_proxy", "all_proxy" })
            {
                Environment.SetEnvironmentVariable(variableName, null);
            }

            EnsureProxyBypass("NO_PROXY", new[] { "localhost", "127.0.0.1", "::1" });
            EnsureProxyBypass("no_proxy", new[] { "localhost", "127.0.0.1", "::1" });
            HttpClient.DefaultProxy = NoProxy.Instance;
            WebRequest.DefaultWebProxy = null;
        }

        private static void EnsureProxyBypass(string variableName, string[] hosts)
        {
            var currentValue = Environment.GetEnvironmentVariable(variableName);
            var entries = (currentValue ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            foreach (var host in hosts)
            {
                if (!entries.Contains(host, StringComparer.OrdinalIgnoreCase))
                {
                    entries.Add(host);
                }
            }

            Environment.SetEnvironmentVariable(variableName, string.Join(",", entries));
        }

        private sealed class NoProxy : IWebProxy
        {
            public static readonly NoProxy Instance = new();

            private NoProxy()
            {
            }

            public System.Net.ICredentials? Credentials { get; set; }

            public Uri? GetProxy(Uri destination) => null;

            public bool IsBypassed(Uri host) => true;
        }
    }
}
