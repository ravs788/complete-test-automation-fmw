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

        /// <summary>
        /// Scroll element into view (Edge headless needs this) and type text.
        /// </summary>
        private void TypeInto(By locator, string text)
        {
            var element = Driver.FindElement(locator);
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            element.Clear();
            element.SendKeys(text);
        }
        private readonly By finishButton = By.Id("finish");

        public CheckoutPage(IWebDriver driver) : base(driver) { }

        public bool IsAtCheckoutInfo()
        {
            return Driver.FindElement(checkoutTitle).Text.Contains("Checkout");
        }

        public void FillCheckoutInformation(string firstName, string lastName, string postalCode)
        {
            TypeInto(firstNameInput, firstName);
            TypeInto(lastNameInput, lastName);
            TypeInto(postalCodeInput, postalCode);
            Driver.FindElement(continueButton).Click();
        }

        public void FinishCheckout()
        {
            Driver.FindElement(finishButton).Click();
        }
    }
}
