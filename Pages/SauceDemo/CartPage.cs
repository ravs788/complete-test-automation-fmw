namespace Pages.SauceDemo;

using Pages.Common;
using Microsoft.Playwright;
using System.Threading.Tasks;

public sealed class CartPage(IPage page) : BasePage(page)
{
    public ILocator Title => Page.Locator(".title");
    public ILocator ItemNames => Page.Locator(".cart_item .inventory_item_name");

    public ILocator Item(string productName) =>
        Page.Locator(".cart_item").Filter(new() { Has = Page.Locator(".inventory_item_name", new() { HasTextString = productName }) });

    public async Task RemoveAsync(string productName) =>
        await Item(productName).GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();

    public async Task CheckoutAsync() => await Page.Locator("#checkout").ClickAsync();
}
