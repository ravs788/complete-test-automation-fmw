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

[AllureSuite("AddToCart")]
[AllureTag("add-to-cart", "ui", "regression")]
[Trait("Category", "regression")]
public class AddToCartTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task CanAddBackpackToCart() => RunAsync(async () =>
    {
        var testData = TestDataLoader.Instance.Load<AddToCartTestData>("AddToCartTests/CanAddBackpackToCart.json");

        var inventory = await LoginAsync(testData.User.Username, testData.User.Password);
        await AllureApi.Step("Assert inventory page loaded after login", async () =>
            await Assertions.Expect(inventory.Title).ToHaveTextAsync("Products"));

        await inventory.AddToCartAsync(testData.Product.Name);
        await inventory.OpenCartAsync();
        var cart = new CartPage(Page);

        await AllureApi.Step("Assert cart page loaded", async () =>
            await Assertions.Expect(cart.Title).ToHaveTextAsync("Your Cart"));
        await AllureApi.Step("Assert product is in the cart", async () =>
            await Assertions.Expect(cart.Item(testData.Product.Name)).ToBeVisibleAsync());
    });
}
