using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace UI.Mobile.Utilities
{
    /// <summary>
    /// Explicit wait helpers for both Mobile Web (IWebDriver) and Native (AppiumDriver implements IWebDriver).
    /// </summary>
    public static class WaitHelper
    {
        public const int DefaultTimeoutSeconds = 15;

        public static IWebElement UntilVisible(IWebDriver driver, By locator, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var wait = NewWait(driver, timeoutSeconds);
            return wait.Until(ExpectedConditions.ElementIsVisible(locator));
        }

        public static IWebElement UntilClickable(IWebDriver driver, By locator, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var wait = NewWait(driver, timeoutSeconds);
            return wait.Until(ExpectedConditions.ElementToBeClickable(locator));
        }

        public static bool UntilInvisible(IWebDriver driver, By locator, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var wait = NewWait(driver, timeoutSeconds);
            return wait.Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
        }

        public static IWebElement UntilExists(IWebDriver driver, By locator, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var wait = NewWait(driver, timeoutSeconds);
            return wait.Until(ExpectedConditions.ElementExists(locator));
        }

        public static void UntilUrlContains(IWebDriver driver, string partial, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var wait = NewWait(driver, timeoutSeconds);
            wait.Until(ExpectedConditions.UrlContains(partial));
        }

        public static IAlert UntilAlertIsPresent(IWebDriver driver, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var wait = NewWait(driver, timeoutSeconds);
            return wait.Until(ExpectedConditions.AlertIsPresent());
        }

        private static WebDriverWait NewWait(IWebDriver driver, int timeoutSeconds)
        {
            var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(timeoutSeconds), TimeSpan.FromMilliseconds(250));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            return wait;
        }
    }
}
