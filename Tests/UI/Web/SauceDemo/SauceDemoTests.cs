using Core.Reporting;
using Core.Ui;
using Domain.TestData.SauceDemo;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[ReportSuite("UI Web", "SauceDemo", "Login")]
[ReportTags("ui", "saucedemo", "login", "smoke", "regression")]
[Trait("Category", "smoke")]
[Trait("Category", "regression")]
public class SauceDemoTests(BrowserFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task Login_Should_ShowInventory() => RunAsync(async () =>
    {
        var inventory = await LoginAsync(UserMother.Standard());

        await Report.Step("Assert inventory page loaded after login", async () =>
            await Ui.Expect(inventory.Title).ToHaveTextAsync("Products"));
        await Report.Step("Assert inventory shows products", async () =>
            await Ui.Expect(inventory.Items.First).ToBeVisibleAsync());
    });
}
