namespace Tests.UI.Web.SauceDemo;

using Pages.SauceDemo;
using Tests.Fixtures;
using Tests.Infrastructure;
using System.Threading.Tasks;

public abstract class SauceDemoTestBase(PlaywrightFixture fixture) : UiTestBase(fixture)
{
    protected override string BaseUrl => Settings.SauceDemo.BaseUrl;

    /// <summary>Opens the login page and signs in; returns the inventory page.</summary>
    protected async Task<InventoryPage> LoginAsync(string username, string password)
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.LoginAsync(username, password);
        return new InventoryPage(Page);
    }
}
