using Microsoft.Playwright;
using Pages.Common;

namespace Pages.SauceDemo;

public sealed class InventoryPage(IPage page) : BasePage(page)
{
    public ILocator Title => Page.Locator(".title");
    public ILocator Items => Page.Locator(".inventory_item");

    private ILocator Item(string productName) =>
        Items.Filter(new() { Has = Page.Locator(".inventory_item_name", new() { HasTextString = productName }) });

    public async Task AddToCartAsync(string productName) =>
        await Item(productName).GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();

    public async Task OpenCartAsync() => await Page.Locator(".shopping_cart_link").ClickAsync();
}
