using Core.Logging;
using Core.Mobile.Configuration;
using Core.Mobile.Interactions;
using Core.Resilience;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Tests.UI.Mobile.Pages.Common;

/// <summary>
/// Base for mobile pages and native screens (Appium; By/AppiumBy locators): waits and retrying actions with logging.
/// </summary>
public abstract class MobileBasePage
{
    protected readonly AppiumDriver Driver;
    protected readonly ILoggingService Logger;

    protected MobileBasePage(AppiumDriver driver, ILoggingService logger)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Navigation

    public void Load(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));

        Logger.Info($"[Nav] Navigating to: {url}");
        Driver.Navigate().GoToUrl(url);
    }

    public string GetPageTitle()
    {
        var title = Driver.Title ?? string.Empty;
        Logger.Info($"[Title] {title}");
        return title;
    }

    // Waits

    protected IWebElement WaitForVisible(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
        => WaitHelper.UntilVisible(Driver, locator, timeoutSeconds);

    protected IWebElement WaitForClickable(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
        => WaitHelper.UntilClickable(Driver, locator, timeoutSeconds);

    protected bool WaitForInvisible(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
        => WaitHelper.UntilInvisible(Driver, locator, timeoutSeconds);

    protected IWebElement WaitForExists(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
        => WaitHelper.UntilExists(Driver, locator, timeoutSeconds);

    // Find helpers

    protected IWebElement Find(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
        => WaitForVisible(locator, timeoutSeconds);

    // Actions (resilient)

    // Momentary UI glitches worth retrying; anything else fails straight away.
    private static bool IsTransientUiError(Exception ex) =>
        ex is StaleElementReferenceException or ElementClickInterceptedException or InvalidElementStateException or WebDriverException;

    private IRetryPolicy UiActionRetry =>
        RetryPolicies.Transient(MobileSettingsProvider.Current.Resilience.UiAction, IsTransientUiError, message => Logger.Debug(message));

    protected void SafeClick(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
    {
        try
        {
            UiActionRetry.Execute(() => WaitForClickable(locator, timeoutSeconds).Click());
            Logger.Info($"[Click] Clicked element: {locator}");
        }
        catch (Exception ex) when (IsTransientUiError(ex))
        {
            Logger.Error($"[Click] Failed to click element: {locator} after retries. Reason: {ex.Message}");
            throw;
        }
    }

    protected void SafeType(By locator, string text, bool clearFirst = true, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
    {
        try
        {
            UiActionRetry.Execute(() =>
            {
                var el = WaitForVisible(locator, timeoutSeconds);
                if (clearFirst)
                    el.Clear();
                el.SendKeys(text);
            });
            Logger.Info($"[Type] Typed into element: {locator} | text length: {text?.Length ?? 0}");
        }
        catch (Exception ex) when (IsTransientUiError(ex))
        {
            Logger.Error($"[Type] Failed to type into element: {locator} after retries. Reason: {ex.Message}");
            throw;
        }
    }

    protected string SafeGetText(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
    {
        var el = WaitForVisible(locator, timeoutSeconds);
        var txt = el.Text ?? string.Empty;
        Logger.Info($"[GetText] Element: {locator} | text length: {txt.Length}");
        return txt;
    }

    protected bool Exists(By locator, int timeoutSeconds = 3)
    {
        try
        {
            WaitForExists(locator, timeoutSeconds);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
