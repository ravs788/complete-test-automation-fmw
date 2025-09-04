using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace UI.Web.Pages
{
    public class CartPage : BasePage
    {
        private readonly By cartTitle = By.ClassName("title");
        private readonly By cartItem = By.ClassName("cart_item");
        private readonly By productName = By.ClassName("inventory_item_name");
        private readonly By checkoutButton = By.Id("checkout");

        public CartPage(IWebDriver driver) : base(driver) { }

        public bool IsLoaded()
        {
            return Driver.FindElement(cartTitle).Text == "Your Cart";
        }

        public List<string> GetProductNames()
        {
            return Driver.FindElements(productName).Select(el => el.Text).ToList();
        }

        public void ClickCheckout()
        {
            Driver.FindElement(checkoutButton).Click();
        }

        public CheckoutPage Checkout()
        {
            Driver.FindElement(checkoutButton).Click();
            return new CheckoutPage(Driver);
        }
    }
}
