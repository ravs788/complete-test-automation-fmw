using System;
using System.Runtime.CompilerServices;
using Core.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using UI.Mobile.Drivers;
using UI.Mobile.Utilities;
using Allure.Net.Commons;

namespace UI.Mobile
{
    /// <summary>
    /// Base class for mobile tests. Wrap each test body in <see cref="Run"/>: it starts the Appium
    /// session, attaches a screenshot to Allure on failure, publishes the result and quits the driver.
    /// </summary>
    public abstract class BaseMobileTest : IDisposable
    {
        protected AppiumDriver? Driver { get; private set; }
        protected ILoggingService Logger { get; }

        protected BaseMobileTest()
        {
            Logger = LoggingServiceFactory.CreateLogger("ui-mobile-logs-{0:yyyy.MM.dd}");
        }

        protected void Run(Action test, [CallerMemberName] string testName = "") =>
            TestRunner.Run(
                new TestRunInfo(Logger, "ui-mobile", GetType(), testName, Browser: "mobile"),
                body: () =>
                {
                    StartSession();
                    test();
                },
                onFailure: AttachFailureScreenshot,
                cleanup: QuitDriver);

        private void StartSession()
        {
            var config = ConfigLoader.Load<ConfigSettings>();
            var (platform, appType) = MobileOptionsBuilder.Parse(config);

            Logger.Info($"[Device] {platform} ({appType}) Name={config.Device.DeviceName}, PlatformVersion={config.Device.PlatformVersion}, UDID={config.Device.Udid}");

            var factory = new MobileDriverFactory();
            Driver = factory.CreateDriver(config, Logger);

            // For Mobile Web, navigate to BaseUrl if provided
            if (appType == MobileAppType.Web && !string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                Driver.Navigate().GoToUrl(config.BaseUrl);
                Logger.Info($"[SetUp] Navigated to {config.BaseUrl} on {platform} {appType}");
            }

            // Append platform/appType to Allure test name (for reporting clarity)
            AllureLifecycle.Instance.UpdateTestCase(x => x.name = $"{x.name} ({platform} {appType})");
        }

        private void AttachFailureScreenshot()
        {
            if (Driver is ITakesScreenshot taker)
            {
                var screenshot = taker.GetScreenshot();
                var filename = $"Test_Failure_{DateTime.Now:yyyyMMddHHmmssfff}.png";
                AllureApi.AddAttachment("Test Failure (Mobile)", "image/png", screenshot.AsByteArray, filename);
            }
        }

        private void QuitDriver()
        {
            try
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
            catch { }
            Driver = null;
        }

        /// <summary>Safety net in case a test didn't go through <see cref="Run"/>.</summary>
        public void Dispose()
        {
            QuitDriver();
            GC.SuppressFinalize(this);
        }
    }
}
