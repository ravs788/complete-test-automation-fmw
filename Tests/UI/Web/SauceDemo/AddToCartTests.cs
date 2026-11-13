using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.SauceDemo;
using Microsoft.Playwright;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[AllureParentSuite("UI Web")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Add to cart")]
[AllureTag("ui", "saucedemo", "add-to-cart", "regression")]
[Trait("Category", "regression")]
public class AddToCartTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task CanAddBackpackToCart() => RunAsync(async () =>
    {
        var product = ProductMother.Backpack();

        var inventory = await LoginAsync(UserMother.Standard());
        await AllureApi.Step("Assert inventory page loaded after login", async () =>
            await Assertions.Expect(inventory.Title).ToHaveTextAsync("Products"));

        await inventory.AddToCartAsync(product.Name);
        var cart = await inventory.OpenCartAsync();

        await AllureApi.Step("Assert cart page loaded", async () =>
            await Assertions.Expect(cart.Title).ToHaveTextAsync("Your Cart"));
        await AllureApi.Step("Assert product is in the cart", async () =>
            await Assertions.Expect(cart.Item(product.Name)).ToBeVisibleAsync());
    });
}
