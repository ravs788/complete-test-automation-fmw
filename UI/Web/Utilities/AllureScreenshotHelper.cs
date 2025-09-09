using Core.Utilities;
using OpenQA.Selenium;
using System;
using System.IO;
using Allure.Net.Commons;

namespace UI.Web.Utilities
{
    /// <summary>
    /// Selenium+Allure-backed implementation of IScreenshotHelper for UI layer.
    /// </summary>
    public class AllureScreenshotHelper : IScreenshotHelper
    {
        private readonly IWebDriver _driver;

        /// <summary>
        /// Constructs the screenshot helper with a Selenium WebDriver.
        /// </summary>
        /// <param name="driver">The Selenium WebDriver instance to use.</param>
        public AllureScreenshotHelper(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        /// <inheritdoc />
        public void CaptureAndAttach(string stepDescription, bool isSuccess)
        {
            try
            {
                if (_driver is ITakesScreenshot screenshotTaker)
                {
                    Screenshot screenshot = screenshotTaker.GetScreenshot();
                    var filename = $"{stepDescription}_{(isSuccess ? "pass" : "fail")}_{DateTime.Now:yyyyMMddHHmmssfff}.png".Replace(' ', '_');
                    AllureApi.AddAttachment(
                        $"{stepDescription} ({(isSuccess ? "Success" : "Failure")})",
                        "image/png",
                        screenshot.AsByteArray,
                        filename
                    );
                }
            }
            catch (Exception ex)
            {
                // Optionally log or swallow screenshot exceptions for robustness
                // Failed to capture or attach screenshot: {ex}
                // Logging to console was removed to avoid unwanted output.
            }
        }
    }
}
