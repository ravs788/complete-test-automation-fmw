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
        // Arrange: use default mobile settings for Android web automation.
        // Act
        var options = MobileOptionsBuilder.Build(new MobileSettings(), MobilePlatform.Android, MobileAppType.Web);

        // Assert
        Assert.Equal(20_000, Capability(options, "appium:adbExecTimeout"));
        Assert.Equal(20_000, Capability(options, "appium:uiautomator2ServerInstallTimeout"));
        Assert.Equal(30_000, Capability(options, "appium:uiautomator2ServerLaunchTimeout"));
    }

    [Fact]
    public void Build_Should_PassConfiguredTimeouts_ToAppium()
    {
        // Arrange
        var settings = new MobileSettings
        {
            Android = new AndroidSettings { AdbExecTimeoutMs = 120_000, ServerInstallTimeoutMs = 120_000, ServerLaunchTimeoutMs = 90_000 }
        };

        // Act
        var options = MobileOptionsBuilder.Build(settings, MobilePlatform.Android, MobileAppType.Web);

        // Assert
        Assert.Equal(120_000, Capability(options, "appium:adbExecTimeout"));
        Assert.Equal(120_000, Capability(options, "appium:uiautomator2ServerInstallTimeout"));
        Assert.Equal(90_000, Capability(options, "appium:uiautomator2ServerLaunchTimeout"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_Should_KeepChromeAppData_Only_When_NoReset(bool noReset)
    {
        // Arrange
        var settings = new MobileSettings { Behavior = new BehaviorSettings { NoReset = noReset } };

        // Act
        var options = MobileOptionsBuilder.Build(settings, MobilePlatform.Android, MobileAppType.Web);

        // Assert
        var chromeOptions = Assert.IsType<Dictionary<string, object>>(Capability(options, "goog:chromeOptions"));
        Assert.Equal(noReset, chromeOptions["androidKeepAppDataDir"]);
    }
}
