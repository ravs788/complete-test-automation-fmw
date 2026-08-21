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
    [AllureSuite("NegativeLogin")]
    [AllureTag("login", "negative", "ui", "regression")]
    [Category("regression")]
    public class NegativeLoginTests : BaseWebTest
    {
        [Test]
        [TestCase("chrome", "Login_InvalidCredentials.json")]
        // TODO: Re-enable after Firefox is installed/configured on the runner.
        // [TestCase("firefox", "Login_InvalidCredentials.json")]
        [TestCase("edge", "Login_InvalidCredentials.json")]
        [TestCase("chrome", "Login_BlankPassword.json")]
        // [TestCase("firefox", "Login_BlankPassword.json")]
        [TestCase("edge", "Login_BlankPassword.json")]
        public void NegativeLogin_ShouldShowProperError(string browser, string dataFile)
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
                    Assert.That(errorDisplayed, Is.True, "Error message was not displayed for invalid login.");
                    Assert.That(
                        errorText.ToLowerInvariant(),
                        Does.Contain(testData.ExpectedError.ToLowerInvariant()),
                        $"Error text did not match expectation. Expected to contain: '{testData.ExpectedError}', Actual: '{errorText}'");
                    Logger.Info($"[{browser}] PASSED: Negative login produced expected error message.");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"[{browser}] FAILED: Negative login assertions failed - {ex.Message}");
                    throw;
                }
            });
        }
    }
}
