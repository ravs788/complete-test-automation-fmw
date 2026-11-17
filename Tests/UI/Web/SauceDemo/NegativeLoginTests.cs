using Allure.Net.Commons.Attributes;
using Core.Reporting;
using Core.Ui;
using Domain.TestData.SauceDemo;
using Pages.SauceDemo;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[AllureParentSuite("UI Web")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Negative login")]
[AllureTag("ui", "saucedemo", "login", "negative", "regression")]
[Trait("Category", "regression")]
public class NegativeLoginTests(BrowserFixture fixture) : SauceDemoTestBase(fixture)
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
    public Task Login_Should_ShowError_When_CredentialsInvalid(string username, string password, string expectedError) => RunAsync(async () =>
    {
        var login = new LoginPage(Session);

        await login.OpenAsync();
        await login.LoginExpectingErrorAsync(username, password);

        await Report.Step("Assert login error displayed", async () =>
        {
            await Ui.Expect(login.ErrorMessage).ToBeVisibleAsync();
            await Ui.Expect(login.ErrorMessage).ToContainTextAsync(expectedError, ignoreCase: true);
        });
    });
}
