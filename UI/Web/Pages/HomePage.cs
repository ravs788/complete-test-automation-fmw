using OpenQA.Selenium;

namespace UI.Web.Pages
{
    public class HomePage : BasePage
    {
        private readonly By homeTitle = By.ClassName("title");

        public HomePage(IWebDriver driver) : base(driver) { }

        public bool IsAtHomePage()
        {
            // On saucedemo.com, the "Products" inventory page is the main landing page after login
            return Driver.FindElement(homeTitle).Text == "Products";
        }
    }
}
