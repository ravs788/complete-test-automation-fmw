using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.Models.SauceDemo;
using Core.TestData.SauceDemo;
using Microsoft.Playwright;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[AllureParentSuite("UI Web")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Checkout")]
[AllureTag("ui", "saucedemo", "checkout", "regression")]
[Trait("Category", "regression")]
public class CheckoutFlowTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task EndToEnd_Checkout_Success() => RunAsync(async () =>
    {
        Product[] products = [ProductMother.Backpack(), ProductMother.BikeLight()];
        var checkoutInfo = CheckoutInfoMother.Standard();

        var inventory = await LoginAsync(UserMother.Standard());
        await AllureApi.Step("Assert inventory page loaded after login", async () =>
            await Assertions.Expect(inventory.Title).ToHaveTextAsync("Products"));

        foreach (var product in products)
        {
            await inventory.AddToCartAsync(product.Name);
        }
        var cart = await inventory.OpenCartAsync();

        await AllureApi.Step("Assert cart contains the selected products", async () =>
        {
            await Assertions.Expect(cart.Title).ToHaveTextAsync("Your Cart");
            await Assertions.Expect(cart.ItemNames).ToHaveTextAsync(products.Select(p => p.Name).ToArray());
        });

        var information = await cart.CheckoutAsync();
        await AllureApi.Step("Assert checkout information page loaded", async () =>
            await Assertions.Expect(information.Title).ToHaveTextAsync("Checkout: Your Information"));

        var overview = await information.FillAndContinueAsync(
            checkoutInfo.FirstName,
            checkoutInfo.LastName,
            checkoutInfo.PostalCode);

        await Assertions.Expect(overview.Title).ToHaveTextAsync("Checkout: Overview");
        var complete = await overview.FinishAsync();
        await AllureApi.Step("Assert order completion message displayed", async () =>
            await Assertions.Expect(complete.Header).ToContainTextAsync("thank you for your order", new() { IgnoreCase = true }));
    });
}
