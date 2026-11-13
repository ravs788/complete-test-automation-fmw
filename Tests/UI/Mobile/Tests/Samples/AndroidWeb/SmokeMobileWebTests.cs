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
    public class SmokeMobileWebTests : BaseMobileTest
    {
        [Fact]
        public void Saucedemo_Homepage_ShouldLoad() => Run(() =>
        {
            // BaseMobileTest SetUp navigates to BaseUrl from Tests/UI/Mobile/config.json for Mobile Web.
            AllureApi.Step("Wait for username field to be visible", () =>
            {
                Driver.Navigate().GoToUrl("https://www.saucedemo.com");
                var username = WaitHelper.UntilVisible(Driver!, By.CssSelector("input#user-name"), 20);
                Assert.NotNull(username);
            });

            AllureApi.Step("Assert page title contains 'Swag Labs'", () =>
            {
                Assert.Contains("Swag Labs", Driver!.Title);
            });
        });
    }
}
