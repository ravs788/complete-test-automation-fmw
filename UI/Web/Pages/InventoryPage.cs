using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace UI.Web.Pages
{
    public class InventoryPage : BasePage
    {
        private readonly By inventoryTitle = By.ClassName("title");
        private readonly By inventoryItem = By.ClassName("inventory_item");
        private readonly By productName = By.ClassName("inventory_item_name");

        public InventoryPage(IWebDriver driver) : base(driver) { }

        public bool IsAtInventoryPage()
        {
            // Robustly wait for the inventory page title to be present
            try
            {
                var el = new WebDriverWait(Driver, TimeSpan.FromSeconds(5)).Until(
                    drv => drv.FindElement(inventoryTitle)
                );
                return el.Text.Trim().Equals("Products", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public int GetInventoryItemCount()
        {
            return Driver.FindElements(inventoryItem).Count;
        }

        public void AddToCart(string productNameText)
        {
            var items = Driver.FindElements(inventoryItem);
            foreach (var item in items)
            {
                var nameEl = item.FindElement(productName);
                if (nameEl.Text == productNameText)
                {
                    item.FindElement(By.TagName("button")).Click(); // Add to cart button
                    return;
                }
            }
            throw new System.Exception($"Product with name '{productNameText}' not found in inventory.");
        }

        public void OpenCart()
        {
            // cart icon has id 'shopping_cart_container'
            Driver.FindElement(By.Id("shopping_cart_container")).Click();
        }
    }
}
