using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.SauceDemo;
using Microsoft.Playwright;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[AllureParentSuite("UI Web")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Remove from cart")]
[AllureTag("ui", "saucedemo", "remove-from-cart", "regression")]
[Trait("Category", "regression")]
public class RemoveFromCartTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task Cart_Should_RemoveProduct_When_RemoveClicked() => RunAsync(async () =>
    {
        var product = ProductMother.Backpack();

        var inventory = await LoginAsync(UserMother.Standard());
        await inventory.AddToCartAsync(product.Name);
        var cart = await inventory.OpenCartAsync();

        await AllureApi.Step("Assert product added to cart", async () =>
            await Assertions.Expect(cart.Item(product.Name)).ToBeVisibleAsync());

        await cart.RemoveAsync(product.Name);

        await AllureApi.Step("Assert product removed from cart", async () =>
            await Assertions.Expect(cart.Item(product.Name)).ToHaveCountAsync(0));
    });
}
