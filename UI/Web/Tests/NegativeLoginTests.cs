namespace UI.Web.Tests
{
    using UI.Web.Pages;
    using UI.Web.Models;
    using Core.Utilities;
    using Allure.Net.Commons.Attributes;
    using Allure.Net.Commons;
    using Xunit;
    using Xunit.Sdk;

    [AllureSuite("NegativeLogin")]
    [AllureTag("login", "negative", "ui", "regression")]
    [Trait("Category", "regression")]
    public class NegativeLoginTests : BaseWebTest
    {
        [Theory]
        [InlineData("chrome", "Login_InvalidCredentials.json")]
        // TODO: Re-enable after Firefox is installed/configured on the runner.
        // [InlineData("firefox", "Login_InvalidCredentials.json")]
        [InlineData("edge", "Login_InvalidCredentials.json")]
        [InlineData("chrome", "Login_BlankPassword.json")]
        // [InlineData("firefox", "Login_BlankPassword.json")]
        [InlineData("edge", "Login_BlankPassword.json")]
        public void NegativeLogin_ShouldShowProperError(string browser, string dataFile) => Run(browser, () =>
        {
            // Arrange
            var testData = TestDataLoader.Instance.Load<NegativeLoginTestData>($"NegativeLoginTests/{dataFile}");
            var loginPage = new LoginPage(Driver!);

            // Act
            Logger.Info($"[{browser}] Attempting negative login scenario using data file '{dataFile}'");
            loginPage.Login(testData.User.Username, testData.User.Password);

            // Assert
            AllureApi.Step($"Assert login error displayed on {browser}", () =>
            {
                bool errorDisplayed = loginPage.IsErrorDisplayed();
                string errorText = loginPage.GetErrorText();

                try
                {
                    Assert.True(errorDisplayed, "Error message was not displayed for invalid login.");
                    Assert.Contains(testData.ExpectedError.ToLowerInvariant(), errorText.ToLowerInvariant());
                    Logger.Info($"[{browser}] PASSED: Negative login produced expected error message.");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Negative login assertions failed - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
