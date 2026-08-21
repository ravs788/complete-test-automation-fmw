using NUnit.Framework;
using OpenQA.Selenium;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Allure.Net.Commons;
using UI.Mobile;
using UI.Mobile.Utilities;

namespace UI.Mobile.Tests.Samples.AndroidWeb
{
    [AllureNUnit]
    [AllureSuite("Mobile Web - Android")]
    [AllureTag("mobile", "web", "android", "smoke")]
    [Parallelizable(ParallelScope.Self)]
    public class NavigateSaucedemoTests : BaseMobileTest
    {
        [Test]
        public void Navigate_To_Saucedemo_ShouldShowLogin()
        {
            // BaseMobileTest SetUp navigates to baseUrl from UI/Mobile/config.json ("https://www.saucedemo.com")

            AllureApi.Step("Wait for username input to be visible", () =>
            {
                Driver.Navigate().GoToUrl("https://www.saucedemo.com");
                var username = WaitHelper.UntilVisible(Driver!, By.CssSelector("input#user-name"), 20);
                Assert.That(username, Is.Not.Null, "Username input should be visible on Saucedemo login page.");
            });

            AllureApi.Step("Verify page title contains 'Swag Labs'", () =>
            {
                Assert.That(Driver!.Title, Does.Contain("Swag Labs"), "Expected Saucedemo title to contain 'Swag Labs'.");
            });
        }
    }
}
