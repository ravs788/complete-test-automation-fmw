using Core.Mobile.Configuration;
using Core.Mobile.Driver;
using OpenQA.Selenium.Appium;

namespace Tests.Unit.Mobile;

public class MobileOptionsBuilderTests
{
    private static object? Capability(AppiumOptions options, string name) => options.ToCapabilities().GetCapability(name);

    [Fact]
    public void Build_Should_UseAppiumsDefaultTimeouts_When_NotConfigured()
    {
        var options = MobileOptionsBuilder.Build(new MobileSettings(), MobilePlatform.Android, MobileAppType.Web);

        Assert.Equal(20_000, Capability(options, "appium:adbExecTimeout"));
        Assert.Equal(20_000, Capability(options, "appium:uiautomator2ServerInstallTimeout"));
        Assert.Equal(30_000, Capability(options, "appium:uiautomator2ServerLaunchTimeout"));
    }

    [Fact]
    public void Build_Should_PassConfiguredTimeouts_ToAppium()
    {
        var settings = new MobileSettings
        {
            Android = new AndroidSettings { AdbExecTimeoutMs = 120_000, ServerInstallTimeoutMs = 120_000, ServerLaunchTimeoutMs = 90_000 }
        };

        var options = MobileOptionsBuilder.Build(settings, MobilePlatform.Android, MobileAppType.Web);

        Assert.Equal(120_000, Capability(options, "appium:adbExecTimeout"));
        Assert.Equal(120_000, Capability(options, "appium:uiautomator2ServerInstallTimeout"));
        Assert.Equal(90_000, Capability(options, "appium:uiautomator2ServerLaunchTimeout"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_Should_KeepChromeAppData_Only_When_NoReset(bool noReset)
    {
        var settings = new MobileSettings { Behavior = new BehaviorSettings { NoReset = noReset } };

        var options = MobileOptionsBuilder.Build(settings, MobilePlatform.Android, MobileAppType.Web);

        var chromeOptions = Assert.IsType<Dictionary<string, object>>(Capability(options, "goog:chromeOptions"));
        Assert.Equal(noReset, chromeOptions["androidKeepAppData"]);
    }
}
