using OpenQA.Selenium;

namespace UI.Web.Pages
{
    public class CheckoutOverviewPage : BasePage
    {
        private readonly By finishButton = By.Id("finish");

        public CheckoutOverviewPage(IWebDriver driver) : base(driver) { }

        public void ClickFinish()
        {
            Driver.FindElement(finishButton).Click();
        }
    }
}
