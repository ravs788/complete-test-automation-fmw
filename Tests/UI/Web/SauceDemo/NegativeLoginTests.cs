namespace Tests.UI.Web.SauceDemo;

using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.SauceDemo;
using Pages.SauceDemo;
using Tests.Fixtures;
using Core.Utilities;
using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

[AllureSuite("NegativeLogin")]
[AllureTag("login", "negative", "ui", "regression")]
[Trait("Category", "regression")]
public class NegativeLoginTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Theory]
    [InlineData("Login_InvalidCredentials.json")]
    [InlineData("Login_BlankPassword.json")]
    public Task NegativeLogin_ShouldShowProperError(string dataFile) => RunAsync(async () =>
    {
        var testData = TestDataLoader.Instance.Load<NegativeLoginTestData>($"NegativeLoginTests/{dataFile}");
        var login = new LoginPage(Page);

        await login.OpenAsync();
        await login.LoginAsync(testData.User.Username, testData.User.Password);

        await AllureApi.Step("Assert login error displayed", async () =>
        {
            await Assertions.Expect(login.ErrorMessage).ToBeVisibleAsync();
            await Assertions.Expect(login.ErrorMessage).ToContainTextAsync(testData.ExpectedError, new() { IgnoreCase = true });
        });
    });
}
