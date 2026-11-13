using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.SauceDemo;
using Microsoft.Playwright;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[AllureParentSuite("UI Web")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Login")]
[AllureTag("ui", "saucedemo", "login", "smoke", "regression")]
[Trait("Category", "smoke")]
[Trait("Category", "regression")]
public class SauceDemoTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task Saucedemo_Login_ShouldShowInventory() => RunAsync(async () =>
    {
        var inventory = await LoginAsync(UserMother.Standard());

        await AllureApi.Step("Assert inventory page loaded after login", async () =>
            await Assertions.Expect(inventory.Title).ToHaveTextAsync("Products"));
        await AllureApi.Step("Assert inventory shows products", async () =>
            await Assertions.Expect(inventory.Items.First).ToBeVisibleAsync());
    });
}
