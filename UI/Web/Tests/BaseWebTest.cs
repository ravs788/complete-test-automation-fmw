using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using Core.Utilities;
using UI.Web.Utilities;
using Allure.Net.Commons;

namespace UI.Web
{
    public abstract class BaseWebTest
    {
        protected IWebDriver? Driver { get; private set; }
        protected IScreenshotHelper? ScreenshotHelper { get; private set; }
        protected ILoggingService Logger { get; private set; }

        private DateTime _testStartTime;

        public BaseWebTest()
        {
            Logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
        }

        [SetUp]
        public virtual void SetUp()
        {
            _testStartTime = DateTime.Now;

            var config = Core.Utilities.ConfigLoader.Load<UI.Web.Utilities.ConfigSettings>();
            string browser = "firefox"; // default

            if (TestContext.CurrentContext.Test.Arguments.Length > 0 && TestContext.CurrentContext.Test.Arguments[0] is string arg)
                browser = arg.ToLowerInvariant();

            Logger.Info($"[SetUp] Starting test '{TestContext.CurrentContext.Test.Name}' on browser '{browser}' (Headless: {config.Headless})");

            switch (browser)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddUserProfilePreference("credentials_enable_Service", false);
                    chromeOptions.AddUserProfilePreference("profile.password_manager_leak_detection", false);
                    if (config.Headless)
                        chromeOptions.AddArgument("--headless=new");
                    Driver = new ChromeDriver(chromeOptions);
                    break;
                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    if (config.Headless)
                        firefoxOptions.AddArgument("--headless");
                    Driver = new FirefoxDriver(firefoxOptions);
                    break;
                case "edge":
                    var edgeOptions = new EdgeOptions();
                    if (config.Headless)
                        edgeOptions.AddArgument("headless");
                    Driver = new EdgeDriver(edgeOptions);
                    break;
                default:
                    var defaultOptions = new FirefoxOptions();
                    if (config.Headless)
                        defaultOptions.AddArgument("--headless");
                    Driver = new FirefoxDriver(defaultOptions);
                    break;
            }
            Driver!.Manage().Window.Maximize();
            Driver.Navigate().GoToUrl(config.BaseUrl);
            Logger.Info($"[SetUp] Browser window maximized and navigated to {config.BaseUrl}");

            ScreenshotHelper = Driver != null
                ? new Utilities.AllureScreenshotHelper(Driver)
                : null;

            // Append browser name to Allure test case name (for reporting clarity)
            AllureLifecycle.Instance.UpdateTestCase(x => x.name = $"{x.name} ({browser})");
        }

        [TearDown]
        public virtual void TearDown()
        {
            DateTime endTime = DateTime.Now;
            AllureLifecycle.Instance.UpdateTestCase(tc =>
            {
                tc.parameters.Add(new Parameter
                {
                    name = "Start Time",
                    value = _testStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Parameter
                {
                    name = "End Time",
                    value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Parameter
                {
                    name = "Duration (s)",
                    value = (endTime - _testStartTime).TotalSeconds.ToString("F3")
                });
            });

            // Log test outcome and duration
            var ctx = TestContext.CurrentContext;
            string browser = (ctx.Test.Arguments.Length > 0 && ctx.Test.Arguments[0] is string b) ? b.ToLowerInvariant() : "firefox";
            Logger.Info($"[TearDown] Finished test '{ctx.Test.Name}' on browser '{browser}' | Outcome: {ctx.Result.Outcome.Status} | Duration(s): {(endTime - _testStartTime).TotalSeconds:F3}");
            if (ctx.Result.Outcome.Status == TestStatus.Failed)
            {
                Logger.Error($"[TearDown] Failure details: {ctx.Result.Message}");
            }

            // Publish result via configured provider
            var metadata = new LogMetadata
            {
                ProjectName = "ui-web",
                TestClassName = ctx.Test.ClassName ?? string.Empty,
                TestMethodName = ctx.Test.MethodName ?? ctx.Test.Name,
                Status = ctx.Result.Outcome.Status.ToString(),
                Duration = (endTime - _testStartTime).TotalSeconds.ToString("F3"),
                Reason = ctx.Result.Message ?? string.Empty,
                RunTime = endTime.ToString("o"),
                RunName = ctx.Test.FullName ?? ctx.Test.Name,
                TriggeredBy = System.Environment.UserName,
                Browser = browser,
                StartTime = _testStartTime,
                EndTime = endTime
            };
            try
            {
                var publisher = ResultsPublisherFactory.Create();
                publisher.Publish(metadata);
            }
            catch (System.Exception ex)
            {
                TestContext.Progress.WriteLine($"[Results] Publish failed: {ex.Message}");
            }


            try
            {
                // Capture screenshot if test failed
                if (ScreenshotHelper != null && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    try
                    {
                        ScreenshotHelper.CaptureAndAttach("Test Failure", false);
                    }
                    catch { }
                }
            }
            finally
            {
                Driver?.Quit();
                Driver?.Dispose();
                Driver = null;
            }
        }
    }
}
