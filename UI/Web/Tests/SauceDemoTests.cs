namespace UI.Web.Tests
{
    using NUnit.Framework;
    using UI.Web.Pages;
    using UI.Web.Models;
    using Core.Utilities;
    using Allure.NUnit.Attributes;
    using Allure.NUnit;
    using Allure.Net.Commons;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Edge;

    [AllureNUnit]
    [AllureSuite("Saucedemo")]
    [AllureTag("saucedemo", "ui", "smoke", "regression")]
    [Category("smoke")]
    [Category("regression")]
    [Parallelizable(ParallelScope.Self)]
    public class SauceDemoTests : BaseWebTest
    {

        [Test]
        [TestCase("chrome")]
        // TODO: Re-enable after Firefox is installed/configured on the runner.
        // [TestCase("firefox")]
        [TestCase("edge")]
        public void Saucedemo_Login_ShouldShowInventory(string browser)
        {
            // Arrange
            var user = TestDataLoader.Instance.Load<User>("SauceDemoTests/Saucedemo_Login_ShouldShowInventory.json");
            var loginPage = new LoginPage(Driver!);

            // Act
            Logger.Info($"[{browser}] Attempting login for user '{user.Username}'");
            loginPage.Login(user.Username, user.Password);
            Logger.Info($"[{browser}] Login submitted");
            var inventoryPage = new InventoryPage(Driver!);

            // Assert
            AllureApi.Step($"Assert inventory page loaded after login on {browser}", () =>
            {
                bool actual = inventoryPage.IsAtInventoryPage();
                try
                {
                    Assert.That(actual, "User did not land on inventory page after login.");
                    Logger.Info($"[{browser}] PASSED: Assert inventory page loaded after login (expected: true, actual: {actual})");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Assert inventory page loaded after login (expected: true, actual: {actual}) - {ex.Message}");
                    throw;
                }
            });
            AllureApi.Step($"Assert inventory item count > 0 on {browser}", () =>
            {
                int count = inventoryPage.GetInventoryItemCount();
                try
                {
                    Assert.That(count, Is.GreaterThan(0), "Inventory item count should be greater than 0.");
                    Logger.Info($"[{browser}] PASSED: Assert inventory item count > 0 (actual: {count})");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Assert inventory item count > 0 (actual: {count}) - {ex.Message}");
                    throw;
                }
            });
        }
    }
}
