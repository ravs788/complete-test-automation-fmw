using Core.Reporting;
using Core.Ui;
using Domain.Models.SauceDemo;
using Domain.TestData.SauceDemo;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[ReportSuite("UI Web", "SauceDemo", "Checkout")]
[ReportTags("ui", "saucedemo", "checkout", "regression")]
[Trait("Category", "regression")]
public class CheckoutFlowTests(BrowserFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task Checkout_Should_CompleteOrder() => RunAsync(async () =>
    {
        // Arrange
        Product[] products = [ProductMother.Backpack(), ProductMother.BikeLight()];
        var checkoutInfo = CheckoutInfoMother.Standard();

        var inventory = await LoginAsync(UserMother.Standard());

        // Assert: confirm the arranged session is on the inventory page.
        await Report.Step("Assert inventory page loaded after login", async () =>
            await Ui.Expect(inventory.Title).ToHaveTextAsync("Products"));

        // Act
        foreach (var product in products)
        {
            await inventory.AddToCartAsync(product.Name);
        }
        var cart = await inventory.OpenCartAsync();

        // Assert
        await Report.Step("Assert cart contains the selected products", async () =>
        {
            await Ui.Expect(cart.Title).ToHaveTextAsync("Your Cart");
            await Ui.Expect(cart.ItemNames).ToHaveTextAsync(products.Select(p => p.Name).ToArray());
        });

        // Act: begin checkout.
        var information = await cart.CheckoutAsync();

        // Assert: the checkout information page is displayed.
        await Report.Step("Assert checkout information page loaded", async () =>
            await Ui.Expect(information.Title).ToHaveTextAsync("Checkout: Your Information"));

        // Act: submit the customer information.
        var overview = await information.FillAndContinueAsync(
            checkoutInfo.FirstName,
            checkoutInfo.LastName,
            checkoutInfo.PostalCode);

        // Assert: the checkout overview is displayed.
        await Ui.Expect(overview.Title).ToHaveTextAsync("Checkout: Overview");

        // Act: finish the order.
        var complete = await overview.FinishAsync();

        // Assert: the order confirmation is displayed.
        await Report.Step("Assert order completion message displayed", async () =>
            await Ui.Expect(complete.Header).ToContainTextAsync("thank you for your order", ignoreCase: true));
    });
}
