using OpenQA.Selenium;
using Allure.Net.Commons.Attributes;
using Allure.Net.Commons;
using UI.Mobile;
using UI.Mobile.Utilities;
using Xunit;

namespace UI.Mobile.Tests.Samples.AndroidWeb
{
    [AllureSuite("Mobile Web - Android")]
    [AllureTag("mobile", "web", "android", "smoke")]
    public class NavigateSaucedemoTests : BaseMobileTest
    {
        [Fact]
        public void Navigate_To_Saucedemo_ShouldShowLogin() => Run(() =>
        {
            // BaseMobileTest SetUp navigates to baseUrl from Tests/UI/Mobile/config.json ("https://www.saucedemo.com")

            AllureApi.Step("Wait for username input to be visible", () =>
            {
                Driver.Navigate().GoToUrl("https://www.saucedemo.com");
                var username = WaitHelper.UntilVisible(Driver!, By.CssSelector("input#user-name"), 20);
                Assert.NotNull(username);
            });

            AllureApi.Step("Verify page title contains 'Swag Labs'", () =>
            {
                Assert.Contains("Swag Labs", Driver!.Title);
            });
        });
    }
}
