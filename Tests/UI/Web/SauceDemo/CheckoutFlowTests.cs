using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.Models.SauceDemo;
using Core.TestData.SauceDemo;
using Microsoft.Playwright;
using Pages.SauceDemo;
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
        await inventory.OpenCartAsync();
        var cart = new CartPage(Page);

        await AllureApi.Step("Assert cart contains the selected products", async () =>
        {
            await Assertions.Expect(cart.Title).ToHaveTextAsync("Your Cart");
            await Assertions.Expect(cart.ItemNames).ToHaveTextAsync(products.Select(p => p.Name).ToArray());
        });

        await cart.CheckoutAsync();
        var information = new CheckoutInformationPage(Page);
        await AllureApi.Step("Assert checkout information page loaded", async () =>
            await Assertions.Expect(information.Title).ToHaveTextAsync("Checkout: Your Information"));

        await information.FillAndContinueAsync(
            checkoutInfo.FirstName,
            checkoutInfo.LastName,
            checkoutInfo.PostalCode);

        var overview = new CheckoutOverviewPage(Page);
        await Assertions.Expect(overview.Title).ToHaveTextAsync("Checkout: Overview");
        await overview.FinishAsync();

        var complete = new CheckoutCompletePage(Page);
        await AllureApi.Step("Assert order completion message displayed", async () =>
            await Assertions.Expect(complete.Header).ToContainTextAsync("thank you for your order", new() { IgnoreCase = true }));
    });
}
