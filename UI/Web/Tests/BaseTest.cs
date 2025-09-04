using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using Core.Utilities;

namespace UI.Web
{
    public abstract class BaseTest
    {
        protected IWebDriver? Driver { get; private set; }

        protected Core.Utilities.IScreenshotHelper? ScreenshotHelper { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            var config = ConfigManager.Instance.Settings;
            var browser = (config.Browser ?? "firefox").ToLowerInvariant();
            switch (browser)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();
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
                    var edgeOptions = new OpenQA.Selenium.Edge.EdgeOptions();
                    if (config.Headless)
                        edgeOptions.AddArgument("headless");
                    Driver = new OpenQA.Selenium.Edge.EdgeDriver(edgeOptions);
                    break;
                default:
                    // Fallback to Firefox
                    var defaultOptions = new FirefoxOptions();
                    if (config.Headless)
                        defaultOptions.AddArgument("--headless");
                    Driver = new FirefoxDriver(defaultOptions);
                    break;
            }
            // Navigate to base URL after driver initialization
            Driver!.Navigate().GoToUrl(config.BaseUrl);

            ScreenshotHelper = Driver != null
                ? new UI.Web.Utilities.AllureScreenshotHelper(Driver)
                : null;
        }

        [TearDown]
        public virtual void TearDown()
        {
            try
            {
                // Capture screenshot if test failed
                if (ScreenshotHelper != null && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    try
                    {
                        ScreenshotHelper.CaptureAndAttach("Test Failure", false);
                    }
                    catch { /* Swallow to avoid masking actual test failure */ }
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
