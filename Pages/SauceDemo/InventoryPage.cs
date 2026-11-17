using Core.Ui;

namespace Pages.SauceDemo;

public sealed class InventoryPage(IUiSession session) : SauceDemoPage(session)
{
    public IElement Items => TestId("inventory-item");

    private IElement Item(string productName) =>
        Items.WithChild(ByTestId("inventory-item-name"), productName);

    public async Task AddToCartAsync(string productName) =>
        await Item(productName).Find(Locate.Role(UiRole.Button, "Add to cart")).ClickAsync();

    public async Task<CartPage> OpenCartAsync()
    {
        await TestId("shopping-cart-link").ClickAsync();
        return new CartPage(Session);
    }
}
