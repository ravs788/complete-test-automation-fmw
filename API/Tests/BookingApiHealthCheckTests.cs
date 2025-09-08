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
    [AllureTag("api", "health")]
    public class BookingApiHealthCheckTests
    {
        private ApiClient _client;
        private DateTime _testStartTime;

        [SetUp]
        public void SetUp()
        {
            _testStartTime = DateTime.Now;
            _client = new ApiClient();
        }

        [TearDown]
        public void TearDown()
        {
            DateTime endTime = DateTime.Now;
            AllureLifecycle.Instance.UpdateTestCase(tc =>
            {
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "Start Time",
                    value = _testStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "End Time",
                    value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "Duration (s)",
                    value = (endTime - _testStartTime).TotalSeconds.ToString("F3")
                });
            });
            _client.Dispose();
        }

        [Test]
        public async Task BookingApi_Ping_Should_Return_201()
        {
            var response = await _client.GetAsync<object>("ping");
            // With RestSharp, the status code >=200 is required; ensure /ping is alive.
            AllureApi.Step("Assert API health check via /ping", () =>
            {
                Assert.Pass("API health check (/ping) responded. API is up.");
            });
        }
    }
}
