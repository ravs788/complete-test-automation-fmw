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
    [AllureSuite("Saucedemo")]
    [AllureTag("saucedemo")]
    [Parallelizable(ParallelScope.Children)]
    public class SauceDemoTests : BaseTest
    {
        [Test]
        public void Saucedemo_Login_ShouldShowInventory()
        {
            // Arrange
            var user = TestDataLoader.Instance.Load<User>("SauceDemoTests/Saucedemo_Login_ShouldShowInventory.json");
            var loginPage = new LoginPage(Driver!);

            // Act
            loginPage.Login(user.Username, user.Password);
            var inventoryPage = new InventoryPage(Driver!);

            // Assert
            AllureApi.Step("Assert inventory page loaded after login", () =>
            {
                Assert.That(inventoryPage.IsAtInventoryPage(), "User did not land on inventory page after login.");
            });
            AllureApi.Step("Assert inventory item count > 0", () =>
            {
                Assert.That(inventoryPage.GetInventoryItemCount(), Is.GreaterThan(0), "Inventory item count should be greater than 0.");
            });
        }
    }
}
