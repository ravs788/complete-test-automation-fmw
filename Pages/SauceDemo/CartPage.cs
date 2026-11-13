using Microsoft.Playwright;

namespace Pages.SauceDemo;

public sealed class CartPage(IPage page) : SauceDemoPage(page)
{
    public ILocator ItemNames => TestId("inventory-item").Locator(TestId("inventory-item-name"));

    public ILocator Item(string productName) =>
        TestId("inventory-item").Filter(new() { Has = TestId("inventory-item-name").Filter(new() { HasTextString = productName }) });

    public async Task RemoveAsync(string productName) =>
        await Item(productName).GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();

    public async Task CheckoutAsync() => await TestId("checkout").ClickAsync();
}
