using OpenQA.Selenium;

namespace UI.Web.Pages
{
    public class CheckoutCompletePage : BasePage
    {
        private readonly By headerText = By.ClassName("complete-header");

        public CheckoutCompletePage(IWebDriver driver) : base(driver) { }

        public string GetCompleteHeaderText()
        {
            return Driver.FindElement(headerText).Text;
        }
    }
}
