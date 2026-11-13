using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Tests.UI.Mobile.Infrastructure;
using Tests.UI.Mobile.Pages.SauceDemo;

namespace Tests.UI.Mobile.SauceDemo;

[AllureParentSuite("UI Mobile")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Login")]
[AllureTag("ui", "mobile", "saucedemo", "login", "smoke")]
[Trait("Category", "smoke")]
public class SauceDemoTests : MobileTestBase
{
    [Fact]
    public void LoginPage_Should_Load() => Run(() =>
    {
        // MobileTestBase has already opened BaseUrl (mobilesettings.json).
        var login = new LoginPage(Driver, Logger);

        AllureApi.Step("Assert username field is visible", () =>
            Assert.True(login.IsUsernameVisible(), "Username field not visible"));
        AllureApi.Step("Assert page title contains 'Swag Labs'", () =>
            Assert.Contains("Swag Labs", login.GetPageTitle()));
    });
}
