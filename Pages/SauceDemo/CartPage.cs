using Core.Ui;

namespace Pages.SauceDemo;

public sealed class CartPage(IUiSession session) : SauceDemoPage(session)
{
    public IElement ItemNames => TestId("inventory-item").Find(ByTestId("inventory-item-name"));

    public IElement Item(string productName) =>
        TestId("inventory-item").WithChild(ByTestId("inventory-item-name"), productName);

    public async Task RemoveAsync(string productName) =>
        await Item(productName).Find(Locate.Role(UiRole.Button, "Remove")).ClickAsync();

    public async Task<CheckoutInformationPage> CheckoutAsync()
    {
        await TestId("checkout").ClickAsync();
        return new CheckoutInformationPage(Session);
    }
}
