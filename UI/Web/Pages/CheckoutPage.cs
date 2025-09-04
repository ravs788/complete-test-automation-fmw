using OpenQA.Selenium;

namespace UI.Web.Pages
{
    public class CheckoutPage : BasePage
    {
        private readonly By checkoutTitle = By.ClassName("title"); // text: "Checkout: Your Information" or "Checkout: Overview"
        private readonly By firstNameInput = By.Id("first-name");
        private readonly By lastNameInput = By.Id("last-name");
        private readonly By postalCodeInput = By.Id("postal-code");
        private readonly By continueButton = By.Id("continue");
        private readonly By finishButton = By.Id("finish");

        public CheckoutPage(IWebDriver driver) : base(driver) { }

        public bool IsAtCheckoutInfo()
        {
            return Driver.FindElement(checkoutTitle).Text.Contains("Checkout");
        }

        public void FillCheckoutInformation(string firstName, string lastName, string postalCode)
        {
            Driver.FindElement(firstNameInput).SendKeys(firstName);
            Driver.FindElement(lastNameInput).SendKeys(lastName);
            Driver.FindElement(postalCodeInput).SendKeys(postalCode);
            Driver.FindElement(continueButton).Click();
        }

        public void FinishCheckout()
        {
            Driver.FindElement(finishButton).Click();
        }
    }
}
