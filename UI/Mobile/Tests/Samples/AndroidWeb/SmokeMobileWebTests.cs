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
    public class SmokeMobileWebTests : BaseMobileTest
    {
        [Test]
        public void Saucedemo_Homepage_ShouldLoad()
        {
            // BaseMobileTest SetUp navigates to BaseUrl from UI/Mobile/config.json for Mobile Web.
            AllureApi.Step("Wait for username field to be visible", () =>
            {
                var username = WaitHelper.UntilVisible(Driver!, By.CssSelector("input#user-name"), 20);
                Assert.That(username, Is.Not.Null, "Username input should be present on Saucedemo login page.");
            });

            AllureApi.Step("Assert page title contains 'Swag Labs'", () =>
            {
                Assert.That(Driver!.Title, Does.Contain("Swag Labs"), "Expected Saucedemo page title to contain 'Swag Labs'.");
            });
        }
    }
}
