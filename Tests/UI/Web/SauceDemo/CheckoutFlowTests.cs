namespace Tests.UI.Web.SauceDemo;

using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.SauceDemo;
using Pages.SauceDemo;
using Tests.Fixtures;
using Core.Utilities;
using Microsoft.Playwright;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

[AllureSuite("CheckoutFlow")]
[AllureTag("checkout-flow", "ui", "regression")]
[Trait("Category", "regression")]
public class CheckoutFlowTests(PlaywrightFixture fixture) : SauceDemoTestBase(fixture)
{
    [Fact]
    public Task EndToEnd_Checkout_Success() => RunAsync(async () =>
    {
        var testData = TestDataLoader.Instance.Load<CheckoutFlowTestData>("CheckoutFlowTests/EndToEnd_Checkout_Success.json");

        var inventory = await LoginAsync(testData.User.Username, testData.User.Password);
        await AllureApi.Step("Assert inventory page loaded after login", async () =>
            await Assertions.Expect(inventory.Title).ToHaveTextAsync("Products"));

        foreach (var product in testData.Products)
        {
            await inventory.AddToCartAsync(product.Name);
        }
        await inventory.OpenCartAsync();
        var cart = new CartPage(Page);

        await AllureApi.Step("Assert cart contains the selected products", async () =>
        {
            await Assertions.Expect(cart.Title).ToHaveTextAsync("Your Cart");
            await Assertions.Expect(cart.ItemNames).ToHaveTextAsync(testData.Products.Select(p => p.Name).ToArray());
        });

        await cart.CheckoutAsync();
        var information = new CheckoutInformationPage(Page);
        await AllureApi.Step("Assert checkout information page loaded", async () =>
            await Assertions.Expect(information.Title).ToHaveTextAsync("Checkout: Your Information"));

        await information.FillAndContinueAsync(
            testData.CheckoutInfo.FirstName,
            testData.CheckoutInfo.LastName,
            testData.CheckoutInfo.PostalCode);

        var overview = new CheckoutOverviewPage(Page);
        await Assertions.Expect(overview.Title).ToHaveTextAsync("Checkout: Overview");
        await overview.FinishAsync();

        var complete = new CheckoutCompletePage(Page);
        await AllureApi.Step("Assert order completion message displayed", async () =>
            await Assertions.Expect(complete.Header).ToContainTextAsync("thank you for your order", new() { IgnoreCase = true }));
    });
}
