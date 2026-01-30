using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using UI.Web.Pages;
using UI.Web.Utilities;

namespace UI.Web.Tests
{
    [AllureNUnit]
    [AllureSuite("Failing UI Tests")]
    [AllureTag("ui", "failing", "regression")]
    [Parallelizable(ParallelScope.All)]
    public class FailingUiTests : BaseWebTest
    {
        [Test]
        [AllureName("Failing UI Test - Assertion Failure")]
        [AllureDescription("This test fails due to an assertion error in UI.")]
        public void FailingTest_AssertionFailure()
        {
            // Navigate to login page
            var config = Core.Utilities.ConfigLoader.Load<UI.Web.Utilities.ConfigSettings>();
            Driver.Navigate().GoToUrl(config.BaseUrl);
            // Simulate assertion failure
            Assert.That(Driver.Title, Is.EqualTo("Incorrect Title"), "Assertion failed: Title does not match.");
        }

        [Test]
        [AllureName("Failing UI Test - Exception Thrown")]
        [AllureDescription("This test fails due to an unhandled exception in UI.")]
        public void FailingTest_ExceptionThrown()
        {
            // Simulate throwing an exception
            throw new NoSuchElementException("Exception thrown: Element not found.");
        }

        [Test]
        [AllureName("Failing UI Test - Timeout Simulation")]
        [AllureDescription("This test fails due to a simulated timeout in UI.")]
        public void FailingTest_Timeout()
        {
            // Simulate a timeout
            System.Threading.Thread.Sleep(100); // Minimal delay
            Assert.Fail("Test failed due to simulated timeout.");
        }
    }
}
