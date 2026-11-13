namespace UI.Web.Tests
{
    using UI.Web.Pages;
    using UI.Web.Models;
    using Core.Utilities;
    using Allure.Net.Commons.Attributes;
    using Allure.Net.Commons;
    using UI.Web.Utilities;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Edge;
    using Xunit;
    using Xunit.Sdk;
    // using Core.Utilities; -- Already above. Don't re-add.

    [AllureSuite("AddToCart")]
    [AllureTag("add-to-cart", "ui", "regression")]
    [Trait("Category", "regression")]
    public class AddToCartTests : BaseWebTest
    {

        [Theory]
        [InlineData("chrome")]
        // TODO: Re-enable after Firefox is installed/configured on the runner.
        // [InlineData("firefox")]
        [InlineData("edge")]
        public void CanAddBackpackToCart(string browser) => Run(browser, () =>
        {
            // Arrange
            var testData = TestDataLoader.Instance.Load<AddToCartTestData>("AddToCartTests/CanAddBackpackToCart.json");
            var loginPage = new LoginPage(Driver!);

            // Act
            loginPage.Login(testData.User.Username, testData.User.Password);

            var inventoryPage = new InventoryPage(Driver!);
            AllureApi.Step($"Assert inventory page loaded after login on {browser}", () =>
            {
                bool actual = inventoryPage.IsAtInventoryPage();
                try
                {
                    Assert.True(actual);
                    Logger.Info($"[{browser}] PASSED: Assert inventory page loaded after login (expected: true, actual: {actual})");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Assert inventory page loaded after login (expected: true, actual: {actual}) - {ex.Message}");
                    throw;
                }
            });

            inventoryPage.AddToCart(testData.Product.Name);
            inventoryPage.OpenCart();
            var cartPage = new CartPage(Driver!);

            AllureApi.Step($"Assert cart page loaded on {browser}", () =>
            {
                bool actual = cartPage.IsLoaded();
                try
                {
                    Assert.True(actual);
                    Logger.Info($"[{browser}] PASSED: Assert cart page loaded (expected: true, actual: {actual})");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Assert cart page loaded (expected: true, actual: {actual}) - {ex.Message}");
                    throw;
                }
            });
            AllureApi.Step($"Assert product is in the cart on {browser}", () =>
            {
                bool contains = cartPage.GetProductNames().Contains(testData.Product.Name);
                try
                {
                    Assert.True(contains);
                    Logger.Info($"[{browser}] PASSED: Assert product '{testData.Product.Name}' is in cart (expected: true, actual: {contains})");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Assert product '{testData.Product.Name}' is in cart (expected: true, actual: {contains}) - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
