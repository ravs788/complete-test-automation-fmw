using OpenQA.Selenium;

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
    }
}
