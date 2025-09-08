namespace UI.Web.Tests
{
    using NUnit.Framework;
    using UI.Web.Pages;
    using UI.Web.Models;
    using Core.Utilities;
    using Allure.NUnit.Attributes;
    using Allure.NUnit;
    using Allure.Net.Commons;
    using UI.Web.Utilities;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Edge;
    // using Core.Utilities; -- Already above. Don't re-add.

    [AllureNUnit]
    [AllureSuite("AddToCart")]
    [AllureTag("add-to-cart", "ui")]
    [Parallelizable(ParallelScope.Self)]
    public class AddToCartTests : BaseTest
    {

        [Test]
        [TestCase("chrome")]
        [TestCase("firefox")]
        [TestCase("edge")]
        public void CanAddBackpackToCart(string browser)
        {
            // Arrange
            var testData = TestDataLoader.Instance.Load<AddToCartTestData>("AddToCartTests/CanAddBackpackToCart.json");
            var loginPage = new LoginPage(Driver!);

            // Act
            loginPage.Login(testData.User.Username, testData.User.Password);

            var inventoryPage = new InventoryPage(Driver!);
            AllureApi.Step($"Assert inventory page loaded after login on {browser}", () =>
            {
                Assert.That(
                    inventoryPage.IsAtInventoryPage(),
                    Is.EqualTo(true)
                );
            });

            inventoryPage.AddToCart(testData.Product.Name);
            inventoryPage.OpenCart();
            var cartPage = new CartPage(Driver!);

            AllureApi.Step($"Assert cart page loaded on {browser}", () =>
            {
                Assert.That(
                    cartPage.IsLoaded(),
                    Is.EqualTo(true)
                );
            });
            AllureApi.Step($"Assert product is in the cart on {browser}", () =>
            {
                bool contains = cartPage.GetProductNames().Contains(testData.Product.Name);
                Assert.That(
                    contains,
                    Is.EqualTo(true)
                );
            });
        }
    }
}
