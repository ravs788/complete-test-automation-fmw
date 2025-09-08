using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utilities;
using API.Models;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace API.Tests
{
    [AllureNUnit]
    [AllureSuite("Booking Post API")]
    [AllureTag("api", "post")]
    public class BookingApiPostTests
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
        public async Task Create_New_Booking_Should_Succeed()
        {
            var newBooking = Core.Utilities.TestDataLoader.Instance.Load<API.Models.Booking>(
                "BookingApiPostTests/HappyPath.json"
            );
            // Randomize a key property to avoid demo API duplication/spam rejection
            newBooking.lastname = $"{newBooking.lastname}_{System.DateTime.UtcNow.Ticks}";
            var postResponse = await _client.PostAsync<Booking, Dictionary<string, object>>("booking", newBooking);
            AllureApi.Step("Assert booking creation response", () =>
            {
                Assert.That(postResponse, Is.Not.Null, "Booking creation failed");
                Assert.That(postResponse.ContainsKey("bookingid"), "Response missing bookingid");
            });
        }
    }
}
