using Domain.Models.SauceDemo;
using Pages.SauceDemo;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.UI.Web.SauceDemo;

public abstract class SauceDemoTestBase(BrowserFixture fixture) : UiTestBase(fixture)
{
    protected override string BaseUrl => Settings.SauceDemo.BaseUrl;

    /// <summary>Opens the login page and signs in as <paramref name="user"/>; returns the inventory page.</summary>
    protected async Task<InventoryPage> LoginAsync(User user)
    {
        var login = new LoginPage(Session);
        await login.OpenAsync();
        return await login.LoginAsync(user.Username, user.Password);
    }
}
