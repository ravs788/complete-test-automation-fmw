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

[AllureSuite("RemoveFromCart")]
[AllureTag("cart", "ui", "regression")]
[Trait("Category", "regression")]
public class RemoveFromCartTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task CanRemoveBackpackFromCart() => RunAsync(async () =>
    {
        var testData = TestDataLoader.Instance.Load<AddToCartTestData>("RemoveFromCartTests/RemoveBackpackFromCart.json");

        var inventory = await LoginAsync(testData.User.Username, testData.User.Password);
        await inventory.AddToCartAsync(testData.Product.Name);
        await inventory.OpenCartAsync();
        var cart = new CartPage(Page);

        await AllureApi.Step("Assert product added to cart", async () =>
            await Assertions.Expect(cart.Item(testData.Product.Name)).ToBeVisibleAsync());

        await cart.RemoveAsync(testData.Product.Name);

        await AllureApi.Step("Assert product removed from cart", async () =>
            await Assertions.Expect(cart.Item(testData.Product.Name)).ToHaveCountAsync(0));
    });
}
