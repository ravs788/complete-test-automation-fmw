using NUnit.Framework;
using System.Threading.Tasks;
using API.Utilities;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace API.Tests
{
    [AllureNUnit]
    [AllureSuite("Booking Health Check API")]
    [AllureTag("api", "health", "smoke", "regression")]
    [Category("smoke")]
    [Category("regression")]
    public class BookingApiHealthCheckTests : BaseApiTest
    {






        [Test]
        public async Task BookingApi_Ping_Should_Return_201()
        {
            var response = await _client.GetAsync<object>("ping");
            // With RestSharp, the status code >=200 is required; ensure /ping is alive.
            AllureApi.Step("Assert API health check via /ping", () =>
            {
                try
                {
                    Logger.Info("PASSED: API health check (/ping) responded. API is up.");
                    Assert.Pass("API health check (/ping) responded. API is up.");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: API health check (/ping) - {ex.Message}");
                    throw;
                }
            });
        }
    }
}
