using Core.Reporting;
using Core.Ui;
using Pages.SauceDemo;
using Tests.UI.Mobile.Infrastructure;

namespace Tests.UI.Mobile.SauceDemo;

[ReportSuite("UI Mobile", "SauceDemo", "Login")]
[ReportTags("ui", "mobile", "saucedemo", "login", "smoke")]
[Trait("Category", "smoke")]
public class SauceDemoTests : MobileTestBase
{
    [Fact]
    public Task LoginPage_Should_Load() => RunAsync(async () =>
    {
        // MobileTestBase has already opened BaseUrl (mobilesettings.json). Same page object as the web suite (R47).
        var login = new LoginPage(Session);

        await Report.Step("Assert username field is visible", () =>
            Ui.Expect(login.Username).ToBeVisibleAsync());
        await Report.Step("Assert page title contains 'Swag Labs'", async () =>
            Assert.Contains("Swag Labs", await Session.TitleAsync()));
    });
}
