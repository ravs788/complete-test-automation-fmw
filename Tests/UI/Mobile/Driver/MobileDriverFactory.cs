using System.Net;
using Core.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using Tests.UI.Mobile.Configuration;

namespace Tests.UI.Mobile.Driver;

/// <summary>Creates an Android or iOS Appium session from <see cref="MobileSettings"/>.</summary>
public static class MobileDriverFactory
{
    public static AppiumDriver CreateDriver(MobileSettings config, ILoggingService logger)
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
