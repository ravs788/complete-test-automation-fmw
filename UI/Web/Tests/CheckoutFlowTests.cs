namespace UI.Web.Tests
{
    using UI.Web.Pages;
    using UI.Web.Models;
    using Core.Utilities;
    using System.Linq;
    using Allure.Net.Commons.Attributes;
    using Allure.Net.Commons;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Edge;
    using Xunit;
    using Xunit.Sdk;

    [AllureSuite("CheckoutFlow")]
    [AllureTag("checkout-flow", "ui", "regression")]
    [Trait("Category", "regression")]
    public class CheckoutFlowTests : BaseWebTest
    {

        [Theory]
        [InlineData("chrome")]
        // TODO: Re-enable after Firefox is installed/configured on the runner.
        // [InlineData("firefox")]
        [InlineData("edge")]
        public void EndToEnd_Checkout_Success(string browser) => Run(browser, () =>
        {
            // Arrange
            var testData = TestDataLoader.Instance.Load<CheckoutFlowTestData>("CheckoutFlowTests/EndToEnd_Checkout_Success.json");
            var loginPage = new LoginPage(Driver!);

            // Act
            loginPage.Login(testData.User.Username, testData.User.Password);
            var inventoryPage = new InventoryPage(Driver!);

            AllureApi.Step($"Assert inventory page loaded after login on {browser}", () =>
            {
                bool actual = inventoryPage.IsAtInventoryPage();
                try
                {
                    Assert.True(actual, "Did not land on inventory page after login.");
                    Logger.Info($"[{browser}] PASSED: Assert inventory page loaded after login (expected: true, actual: {actual})");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Assert inventory page loaded after login (expected: true, actual: {actual}) - {ex.Message}");
                    throw;
                }
            });

            foreach (var prod in testData.Products)
                inventoryPage.AddToCart(prod.Name);

            inventoryPage.OpenCart();
            var cartPage = new CartPage(Driver!);

            AllureApi.Step($"Assert cart page loaded and contains correct products on {browser}", () =>
            {
                bool isLoaded = cartPage.IsLoaded();
                var names = cartPage.GetProductNames();
                var expectedNames = testData.Products.Select(p => p.Name);
                try
                {
                    Assert.True(isLoaded, "Cart page did not load.");
                    // Cart must contain every expected product.
                    Assert.Superset(expectedNames.ToHashSet(), names.ToHashSet());
                    Logger.Info($"[{browser}] PASSED: Assert cart page loaded (IsLoaded: {isLoaded}) and contains correct products (expected: [{string.Join(",", expectedNames)}], actual: [{string.Join(",", names)}])");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Cart page load/products assertion - {ex.Message} (IsLoaded: {isLoaded}, expected: [{string.Join(",", expectedNames)}], actual: [{string.Join(",", names)}])");
                    throw;
                }
            });

            cartPage.ClickCheckout();
            var checkoutPage = new CheckoutPage(Driver!);
            checkoutPage.FillCheckoutInformation(
                testData.CheckoutInfo.FirstName,
                testData.CheckoutInfo.LastName,
                testData.CheckoutInfo.PostalCode
            );

            AllureApi.Step($"Assert checkout info page loaded on {browser}", () =>
            {
                bool actual = checkoutPage.IsAtCheckoutInfo();
                try
                {
                    Assert.True(actual, "Checkout info page did not load.");
                    Logger.Info($"[{browser}] PASSED: Assert checkout info page loaded (expected: true, actual: {actual})");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Assert checkout info page loaded (expected: true, actual: {actual}) - {ex.Message}");
                    throw;
                }
            });

            var overviewPage = new CheckoutOverviewPage(Driver!);
            overviewPage.ClickFinish();

            var completePage = new CheckoutCompletePage(Driver!);
            AllureApi.Step($"Assert order completion message displayed on {browser}", () =>
            {
                var headerText = completePage.GetCompleteHeaderText();
                try
                {
                    Assert.Contains("thank you for your order", headerText.ToLower());
                    Logger.Info($"[{browser}] PASSED: Order completion message displayed. (Header: '{headerText}')");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Order completion message assertion. (Header: '{headerText}') - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
