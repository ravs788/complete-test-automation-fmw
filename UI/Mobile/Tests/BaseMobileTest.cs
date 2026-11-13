using System;
using Core.Utilities;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using UI.Mobile.Drivers;
using UI.Mobile.Utilities;
using Allure.Net.Commons;

namespace UI.Mobile
{
    public abstract class BaseMobileTest
    {
        protected AppiumDriver? Driver { get; private set; }
        protected ILoggingService Logger { get; private set; }

        private DateTime _testStartTime;

        protected BaseMobileTest()
        {
            Logger = LoggingServiceFactory.CreateLogger("ui-mobile-logs-{0:yyyy.MM.dd}");
        }

        [SetUp]
        public virtual void SetUp()
        {
            _testStartTime = DateTime.Now;

            var config = ConfigLoader.Load<ConfigSettings>();
            var (platform, appType) = MobileOptionsBuilder.Parse(config);

            Logger.Info($"[SetUp] Starting test '{TestContext.CurrentContext.Test.Name}' on {platform} ({appType})");
            Logger.Info($"[Device] Name={config.Device.DeviceName}, PlatformVersion={config.Device.PlatformVersion}, UDID={config.Device.Udid}");

            var factory = new MobileDriverFactory();
            Driver = factory.CreateDriver(config, Logger);

            try
            {
                // For Mobile Web, navigate to BaseUrl if provided
                if (appType == MobileAppType.Web && !string.IsNullOrWhiteSpace(config.BaseUrl))
                {
                    Driver.Navigate().GoToUrl(config.BaseUrl);
                    Logger.Info($"[SetUp] Navigated to {config.BaseUrl} on {platform} {appType}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[SetUp] Navigation or init error: {ex.Message}");
                throw;
            }

            // Append platform/appType to Allure test name (for reporting clarity)
            AllureLifecycle.Instance.UpdateTestCase(x => x.name = $"{x.name} ({platform} {appType})");
        }

        [TearDown]
        public virtual void TearDown()
        {
            var endTime = DateTime.Now;

            // Add timing details to Allure
            AllureLifecycle.Instance.UpdateTestCase(tc =>
            {
                tc.parameters.Add(new Parameter { name = "Start Time", value = _testStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                tc.parameters.Add(new Parameter { name = "End Time", value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                tc.parameters.Add(new Parameter { name = "Duration (s)", value = (endTime - _testStartTime).TotalSeconds.ToString("F3") });
            });

            var ctx = TestContext.CurrentContext;
            Logger.Info($"[TearDown] Finished test '{ctx.Test.Name}' | Outcome: {ctx.Result.Outcome.Status} | Duration(s): {(endTime - _testStartTime).TotalSeconds:F3}");
            if (ctx.Result.Outcome.Status == TestStatus.Failed)
            {
                Logger.Error($"[TearDown] Failure details: {ctx.Result.Message}");
            }

            // Publish result via configured provider
            var metadata = new LogMetadata
            {
                ProjectName = "ui-mobile",
                TestClassName = ctx.Test.ClassName ?? string.Empty,
                TestMethodName = ctx.Test.MethodName ?? ctx.Test.Name,
                Status = ctx.Result.Outcome.Status.ToString(),
                Duration = (endTime - _testStartTime).TotalSeconds.ToString("F3"),
                Reason = ctx.Result.Message ?? string.Empty,
                RunTime = endTime.ToString("o"),
                RunName = ctx.Test.FullName ?? ctx.Test.Name,
                TriggeredBy = Environment.UserName,
                Browser = "mobile", // placeholder field reuse
                StartTime = _testStartTime,
                EndTime = endTime
            };
            try
            {
                var publisher = ResultsPublisherFactory.Create();
                publisher.Publish(metadata);
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"[Results] Publish failed: {ex.Message}");
            }

            try
            {
                // Capture screenshot on failure without requiring a separate helper class
                if (ctx.Result.Outcome.Status == TestStatus.Failed && Driver is ITakesScreenshot taker)
                {
                    try
                    {
                        var screenshot = taker.GetScreenshot();
                        var filename = $"Test_Failure_{DateTime.Now:yyyyMMddHHmmssfff}.png";
                        AllureApi.AddAttachment("Test Failure (Mobile)", "image/png", screenshot.AsByteArray, filename);
                    }
                    catch { /* Swallow to avoid masking the original failure */ }
                }
            }
            finally
            {
                try
                {
                    Driver?.Quit();
                    Driver?.Dispose();
                }
                catch { }
                Driver = null;
            }
        }
    }
}
