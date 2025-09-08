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
    public abstract class BaseTest
    {
        protected IWebDriver? Driver { get; private set; }
        protected IScreenshotHelper? ScreenshotHelper { get; private set; }

        private DateTime _testStartTime;

        [SetUp]
        public virtual void SetUp()
        {
            _testStartTime = DateTime.Now;
            TestContext.WriteLine($"=== Test Start Time: {_testStartTime:yyyy-MM-dd HH:mm:ss.fff} ===");

            var config = ConfigManager.Instance.Settings;
            string browser = "firefox"; // default

            if (TestContext.CurrentContext.Test.Arguments.Length > 0 && TestContext.CurrentContext.Test.Arguments[0] is string arg)
                browser = arg.ToLowerInvariant();

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
