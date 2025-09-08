using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace UI.Web.Pages
{
    public class BasePage
    {
        protected readonly IWebDriver Driver;

        public BasePage(IWebDriver driver)
        {
            Driver = driver;
        }

        public void Load(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        public string GetPageTitle()
        {
            return Driver.Title;
        }

        public void WaitForElementVisible(By locator, int timeoutSeconds = 10)
        {
            new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
                .Until(ExpectedConditions.ElementIsVisible(locator));
        }
    }
}
