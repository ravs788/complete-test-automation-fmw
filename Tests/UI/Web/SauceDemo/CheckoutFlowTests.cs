using Allure.Net.Commons.Attributes;
using Core.Reporting;
using Core.Ui;
using Domain.Models.SauceDemo;
using Domain.TestData.SauceDemo;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[AllureParentSuite("UI Web")]
[AllureSuite("SauceDemo")]
[AllureSubSuite("Checkout")]
[AllureTag("ui", "saucedemo", "checkout", "regression")]
[Trait("Category", "regression")]
public class CheckoutFlowTests(BrowserFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task Checkout_Should_CompleteOrder() => RunAsync(async () =>
    {
        Product[] products = [ProductMother.Backpack(), ProductMother.BikeLight()];
        var checkoutInfo = CheckoutInfoMother.Standard();

        var inventory = await LoginAsync(UserMother.Standard());
        await Report.Step("Assert inventory page loaded after login", async () =>
            await Ui.Expect(inventory.Title).ToHaveTextAsync("Products"));

        foreach (var product in products)
        {
            await inventory.AddToCartAsync(product.Name);
        }
        var cart = await inventory.OpenCartAsync();

        await Report.Step("Assert cart contains the selected products", async () =>
        {
            await Ui.Expect(cart.Title).ToHaveTextAsync("Your Cart");
            await Ui.Expect(cart.ItemNames).ToHaveTextAsync(products.Select(p => p.Name).ToArray());
        });

        var information = await cart.CheckoutAsync();
        await Report.Step("Assert checkout information page loaded", async () =>
            await Ui.Expect(information.Title).ToHaveTextAsync("Checkout: Your Information"));

        var overview = await information.FillAndContinueAsync(
            checkoutInfo.FirstName,
            checkoutInfo.LastName,
            checkoutInfo.PostalCode);

        await Ui.Expect(overview.Title).ToHaveTextAsync("Checkout: Overview");
        var complete = await overview.FinishAsync();
        await Report.Step("Assert order completion message displayed", async () =>
            await Ui.Expect(complete.Header).ToContainTextAsync("thank you for your order", ignoreCase: true));
    });
}
