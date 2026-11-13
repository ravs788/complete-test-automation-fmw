using Microsoft.Playwright;

namespace Pages.SauceDemo;

public sealed class InventoryPage(IPage page) : SauceDemoPage(page)
{
    public ILocator Items => TestId("inventory-item");

    private ILocator Item(string productName) =>
        Items.Filter(new() { Has = TestId("inventory-item-name").Filter(new() { HasTextString = productName }) });

    public async Task AddToCartAsync(string productName) =>
        await Item(productName).GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();

    public async Task OpenCartAsync() => await TestId("shopping-cart-link").ClickAsync();
}
