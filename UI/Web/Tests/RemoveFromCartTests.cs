namespace UI.Web.Tests
{
    using NUnit.Framework;
    using UI.Web.Pages;
    using UI.Web.Models;
    using Core.Utilities;
    using Allure.NUnit.Attributes;
    using Allure.NUnit;
    using Allure.Net.Commons;

    [AllureNUnit]
    [AllureSuite("RemoveFromCart")]
    [AllureTag("cart", "ui", "regression")]
    [Category("regression")]
    public class RemoveFromCartTests : BaseWebTest
    {
        [Test]
        [TestCase("chrome")]
        // TODO: Re-enable after Firefox is installed/configured on the runner.
        // [TestCase("firefox")]
        [TestCase("edge")]
        public void CanRemoveBackpackFromCart(string browser)
        {
            // Arrange
            var testData = TestDataLoader.Instance.Load<AddToCartTestData>("RemoveFromCartTests/RemoveBackpackFromCart.json");
            var loginPage = new LoginPage(Driver!);

            // Act
            loginPage.Login(testData.User.Username, testData.User.Password);
            var inventoryPage = new InventoryPage(Driver!);

            inventoryPage.AddToCart(testData.Product.Name);
            inventoryPage.OpenCart();
            var cartPage = new CartPage(Driver!);

            // Assert product added
            AllureApi.Step($"Assert product added to cart on {browser}", () =>
            {
                bool contains = cartPage.ContainsProduct(testData.Product.Name);
                try
                {
                    Assert.That(contains, Is.True, $"Cart should contain '{testData.Product.Name}' before removal.");
                    Logger.Info($"[{browser}] PASSED: Product '{testData.Product.Name}' present in cart prior to removal.");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"[{browser}] FAILED: {ex.Message}");
                    throw;
                }
            });

            // Remove and assert gone
            cartPage.RemoveProduct(testData.Product.Name);

            AllureApi.Step($"Assert product removed from cart on {browser}", () =>
            {
                bool contains = cartPage.ContainsProduct(testData.Product.Name);
                try
                {
                    Assert.That(contains, Is.False, $"Cart should not contain '{testData.Product.Name}' after removal.");
                    Logger.Info($"[{browser}] PASSED: Product '{testData.Product.Name}' successfully removed from cart.");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"[{browser}] FAILED: {ex.Message}");
                    throw;
                }
            });
        }
    }
}
