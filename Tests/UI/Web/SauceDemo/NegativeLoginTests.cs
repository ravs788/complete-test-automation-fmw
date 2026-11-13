using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.SauceDemo;
using Microsoft.Playwright;
using Pages.SauceDemo;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[AllureParentSuite("UI Web")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Negative login")]
[AllureTag("ui", "saucedemo", "login", "negative", "regression")]
[Trait("Category", "regression")]
public class NegativeLoginTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    // Built from the Builders/Mothers. The rows are plain strings so that xUnit v2 can serialize them and
    // reports each case as its own test (it can't serialize a User record).
    public static TheoryData<string, string, string> InvalidLogins()
    {
        var wrongPassword = new UserBuilder().WithUsername(UserMother.LockedOut().Username).WithPassword("wrong_password").Build();
        var blankPassword = new UserBuilder().WithPassword("").Build();
        return new()
        {
            { wrongPassword.Username, wrongPassword.Password, "Username and password do not match" },
            { blankPassword.Username, blankPassword.Password, "Password is required" },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidLogins))]
    public Task NegativeLogin_ShouldShowProperError(string username, string password, string expectedError) => RunAsync(async () =>
    {
        var login = new LoginPage(Page);

        await login.OpenAsync();
        await login.LoginAsync(username, password);

        await AllureApi.Step("Assert login error displayed", async () =>
        {
            await Assertions.Expect(login.ErrorMessage).ToBeVisibleAsync();
            await Assertions.Expect(login.ErrorMessage).ToContainTextAsync(expectedError, new() { IgnoreCase = true });
        });
    });
}
