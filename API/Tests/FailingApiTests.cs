using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using System;
using API.Utilities;

namespace API.Tests
{
    [AllureNUnit]
    [AllureSuite("Failing API Tests")]
    [AllureTag("api", "failing", "regression")]
    [Parallelizable(ParallelScope.All)]
    public class FailingApiTests : BaseApiTest
    {
        [Test]
        [AllureName("Failing API Test - Assertion Failure")]
        [AllureDescription("This test fails due to an assertion error.")]
        public void FailingTest_AssertionFailure()
        {
            // Simulate an API call or logic that leads to assertion failure
            int expected = 1;
            int actual = 2;
            Assert.That(actual, Is.EqualTo(expected), "Assertion failed: Values do not match.");
        }

        [Test]
        [AllureName("Failing API Test - Exception Thrown")]
        [AllureDescription("This test fails due to an unhandled exception.")]
        public void FailingTest_ExceptionThrown()
        {
            // Simulate throwing an exception
            throw new InvalidOperationException("Exception thrown: Operation is invalid.");
        }

        [Test]
        [AllureName("Failing API Test - Timeout Simulation")]
        [AllureDescription("This test fails due to a simulated timeout.")]
        public void FailingTest_Timeout()
        {
            // Simulate a timeout by failing an assertion after delay (but for simplicity, just fail)
            System.Threading.Thread.Sleep(100); // Minimal delay
            Assert.Fail("Test failed due to simulated timeout.");
        }
    }
}
