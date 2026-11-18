using Core.Reporting;
using Core.Ui;
using Domain.TestData.SauceDemo;
using Tests.Fixtures;

namespace Tests.UI.Web.SauceDemo;

[ReportSuite("UI Web", "SauceDemo", "Add to cart")]
[ReportTags("ui", "saucedemo", "add-to-cart", "regression")]
[Trait("Category", "regression")]
public class AddToCartTests(BrowserFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task Cart_Should_ContainProduct_When_ProductAdded() => RunAsync(async () =>
    {
        // Arrange
        var product = ProductMother.Backpack();

        var inventory = await LoginAsync(UserMother.Standard());

        // Assert: confirm the arranged session is on the inventory page.
        await Report.Step("Assert inventory page loaded after login", async () =>
            await Ui.Expect(inventory.Title).ToHaveTextAsync("Products"));

        // Act
        await inventory.AddToCartAsync(product.Name);
        var cart = await inventory.OpenCartAsync();

        // Assert
        await Report.Step("Assert cart page loaded", async () =>
            await Ui.Expect(cart.Title).ToHaveTextAsync("Your Cart"));
        await Report.Step("Assert product is in the cart", async () =>
            await Ui.Expect(cart.Item(product.Name)).ToBeVisibleAsync());
    });
}
