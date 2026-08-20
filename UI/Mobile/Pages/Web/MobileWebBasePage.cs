using System;
using Core.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using UI.Mobile.Utilities;

namespace UI.Mobile.Pages.Web
{
    /// <summary>
    /// Base page for Mobile Web (Chrome/Safari) running via Appium.
    /// Provides resilient waits and actions using IWebDriver surface of AppiumDriver.
    /// </summary>
    public abstract class MobileWebBasePage
    {
        protected readonly AppiumDriver Driver;
        protected readonly ILoggingService Logger;

        protected MobileWebBasePage(AppiumDriver driver, ILoggingService logger)
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

        protected void SafeClick(By locator, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
        {
            var attempts = 0;
            Exception? last = null;
            while (attempts++ < 3)
            {
                try
                {
                    var el = WaitForClickable(locator, timeoutSeconds);
                    el.Click();
                    Logger.Info($"[Click] Clicked element: {locator}");
                    return;
                }
                catch (Exception ex) when (ex is StaleElementReferenceException || ex is ElementClickInterceptedException || ex is WebDriverException)
                {
                    last = ex;
                    Logger.Debug($"[Click] Retry {attempts} for {locator} due to: {ex.Message}");
                }
            }
            Logger.Error($"[Click] Failed to click element: {locator} after retries. Reason: {last?.Message}");
            throw last ?? new WebDriverException("Unknown click failure");
        }

        protected void SafeType(By locator, string text, bool clearFirst = true, int timeoutSeconds = WaitHelper.DefaultTimeoutSeconds)
        {
            var attempts = 0;
            Exception? last = null;
            while (attempts++ < 3)
            {
                try
                {
                    var el = WaitForVisible(locator, timeoutSeconds);
                    if (clearFirst)
                        el.Clear();
                    el.SendKeys(text);
                    Logger.Info($"[Type] Typed into element: {locator} | text length: {text?.Length ?? 0}");
                    return;
                }
                catch (Exception ex) when (ex is StaleElementReferenceException || ex is InvalidElementStateException || ex is WebDriverException)
                {
                    last = ex;
                    Logger.Debug($"[Type] Retry {attempts} for {locator} due to: {ex.Message}");
                }
            }
            Logger.Error($"[Type] Failed to type into element: {locator} after retries. Reason: {last?.Message}");
            throw last ?? new WebDriverException("Unknown type failure");
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
}
