using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
using System;

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
            try
            {
                WaitForElementVisible(cartTitle, 10);
                return Driver.FindElement(cartTitle).Text == "Your Cart";
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetProductNames()
        {
            return Driver.FindElements(productName).Select(el => el.Text).ToList();
        }

        public bool ContainsProduct(string productNameText)
        {
            return Driver.FindElements(productName).Any(el => el.Text == productNameText);
        }

        public void RemoveProduct(string productNameText)
        {
            var items = Driver.FindElements(cartItem);
            foreach (var item in items)
            {
                var nameEl = item.FindElement(productName);
                if (nameEl.Text == productNameText)
                {
                    item.FindElement(By.TagName("button")).Click(); // Remove button
                    return;
                }
            }
            throw new Exception($"Product with name '{productNameText}' not found in cart.");
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
