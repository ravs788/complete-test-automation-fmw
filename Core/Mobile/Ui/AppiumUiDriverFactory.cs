using Core.Logging;
using Core.Mobile.Configuration;
using Core.Mobile.Driver;
using Core.Ui;

namespace Core.Mobile.Ui;

/// <summary>
/// The UI contract on Appium (<c>appium</c>, R47): each session is a new Appium session built from
/// <c>mobilesettings.json</c> (<see cref="MobileSettingsProvider"/>); the launch options' browser name is not used.
/// </summary>
public sealed class AppiumUiDriverFactory : IUiDriverFactory
{
    public Task<IUiDriver> LaunchAsync(UiLaunchOptions options) => Task.FromResult<IUiDriver>(new AppiumUiDriver());
}

public sealed class AppiumUiDriver : IUiDriver
{
    public Task<IUiSession> NewSessionAsync(UiSessionOptions options)
    {
        var logger = LoggingServiceFactory.CreateLogger("ui-mobile-logs-{0:yyyy.MM.dd}");
        var driver = MobileDriverFactory.CreateDriver(MobileSettingsProvider.Current, logger);
        return Task.FromResult<IUiSession>(new AppiumUiSession(driver, options));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
