namespace Tests.UI.Web.SauceDemo;

using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.SauceDemo;
using Tests.Fixtures;
using Core.Utilities;
using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

[AllureSuite("Saucedemo")]
[AllureTag("saucedemo", "ui", "smoke", "regression")]
[Trait("Category", "smoke")]
[Trait("Category", "regression")]
public class SauceDemoTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task Saucedemo_Login_ShouldShowInventory() => RunAsync(async () =>
    {
        var user = TestDataLoader.Instance.Load<User>("SauceDemoTests/Saucedemo_Login_ShouldShowInventory.json");

        var inventory = await LoginAsync(user.Username, user.Password);

        await AllureApi.Step("Assert inventory page loaded after login", async () =>
            await Assertions.Expect(inventory.Title).ToHaveTextAsync("Products"));
        await AllureApi.Step("Assert inventory shows products", async () =>
            await Assertions.Expect(inventory.Items.First).ToBeVisibleAsync());
    });
}
