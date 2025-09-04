namespace UI.Web.Tests
{
    using NUnit.Framework;
    using UI.Web.Pages;
    using UI.Web.Models;
    using Core.Utilities;
    using System.Linq;
    using Allure.NUnit.Attributes;
    using Allure.NUnit;
    using Allure.Net.Commons;

    [AllureNUnit]
    [AllureSuite("CheckoutFlow")]
    [AllureTag("checkout-flow", "ui")]
    [Parallelizable(ParallelScope.Self)]
    public class CheckoutFlowTests : BaseTest
    {
        [Test]
        public void EndToEnd_Checkout_Success()
        {
            // Arrange
            var testData = TestDataLoader.Instance.Load<CheckoutFlowTestData>("CheckoutFlowTests/EndToEnd_Checkout_Success.json");
            var loginPage = new LoginPage(Driver!);

            // Act
            loginPage.Login(testData.User.Username, testData.User.Password);
            var inventoryPage = new InventoryPage(Driver!);

            AllureApi.Step("Assert inventory page loaded after login", () =>
            {
                Assert.That(inventoryPage.IsAtInventoryPage(), "Did not land on inventory page after login.");
            });

            foreach (var prod in testData.Products)
                inventoryPage.AddToCart(prod.Name);

            inventoryPage.OpenCart();
            var cartPage = new CartPage(Driver!);

            AllureApi.Step("Assert cart page loaded and contains correct products", () =>
            {
                Assert.That(cartPage.IsLoaded(), "Cart page did not load.");
                Assert.That(
                    cartPage.GetProductNames(),
                    Is.SupersetOf(testData.Products.Select(p => p.Name)),
                    "Cart does not contain correct products"
                );
            });

            cartPage.ClickCheckout();
            var checkoutPage = new CheckoutPage(Driver!);
            checkoutPage.FillCheckoutInformation(
                testData.CheckoutInfo.FirstName,
                testData.CheckoutInfo.LastName,
                testData.CheckoutInfo.PostalCode
            );

            AllureApi.Step("Assert checkout info page loaded", () =>
            {
                Assert.That(checkoutPage.IsAtCheckoutInfo(), "Checkout info page did not load.");
            });

            var overviewPage = new CheckoutOverviewPage(Driver!);
            overviewPage.ClickFinish();

            var completePage = new CheckoutCompletePage(Driver!);
            AllureApi.Step("Assert order completion message displayed", () =>
            {
                Assert.That(
                    completePage.GetCompleteHeaderText().ToLower(),
                    Does.Contain("thank you for your order"),
                    "Order completion message not displayed."
                );
            });
        }

    }
}
