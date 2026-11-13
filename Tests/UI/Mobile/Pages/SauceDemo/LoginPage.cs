using Core.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using Tests.UI.Mobile.Pages.Common;

namespace Tests.UI.Mobile.Pages.SauceDemo;

public sealed class LoginPage(AppiumDriver driver, ILoggingService logger) : MobileBasePage(driver, logger)
{
    private static readonly By Username = By.CssSelector("#user-name");

    public bool IsUsernameVisible(int timeoutSeconds = 20) => Exists(Username, timeoutSeconds);
}
