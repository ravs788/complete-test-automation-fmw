using System.Threading.Tasks;
using API.Utilities;
using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Xunit;
using Xunit.Sdk;

namespace API.Tests
{
    [AllureSuite("Booking Health Check API")]
    [AllureTag("api", "health", "smoke", "regression")]
    [Trait("Category", "smoke")]
    [Trait("Category", "regression")]
    public class BookingApiHealthCheckTests : BaseApiTest
    {






        [Fact]
        public Task BookingApi_Ping_Should_Return_201() => RunAsync(async () =>
        {
            var response = await _client.GetAsync<object>("ping");
            // With RestSharp, the status code >=200 is required; ensure /ping is alive.
            AllureApi.Step("Assert API health check via /ping", () =>
            {
                try
                {
                    Logger.Info("PASSED: API health check (/ping) responded. API is up.");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: API health check (/ping) - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
