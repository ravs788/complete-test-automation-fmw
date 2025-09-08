using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utilities;
using API.Models;
using Core.Utilities;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace API.Tests
{
    [AllureNUnit]
    [AllureSuite("Booking Get API")]
    [AllureTag("api", "get")]
    public class BookingApiGetTests
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
                tc.parameters.Add(new Parameter
                {
                    name = "Start Time",
                    value = _testStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Parameter
                {
                    name = "End Time",
                    value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Parameter
                {
                    name = "Duration (s)",
                    value = (endTime - _testStartTime).TotalSeconds.ToString("F3")
                });
            });
            _client.Dispose();
        }

        [Test]
        public async Task Get_All_Booking_Ids()
        {
            // /booking returns array of objects with bookingid fields
            var bookingIds = await _client.GetAsync<List<Dictionary<string, int>>>("booking");
            AllureApi.Step("Assert that bookingIds are returned", () =>
            {
                Assert.That(bookingIds, Is.Not.Null.And.Not.Empty, "Booking IDs should be returned");
            });
        }

        [Test]
        public async Task Get_Booking_By_Id()
        {
            // Create a new booking from test-data
            var newBooking = TestDataLoader.Instance.Load<Booking>(
                "BookingApiGetTests/ById.json"
            );

            // POST /booking (returns bookingid and booking)
            var postResponse = await _client.PostAsync<Booking, Dictionary<string, object>>("booking", newBooking);
            AllureApi.Step("Assert booking created for get-by-id test", () =>
            {
                Assert.That(postResponse, Is.Not.Null, "Booking creation failed");
                Assert.That(postResponse.ContainsKey("bookingid"), "Post response does not include bookingid");
            });

            int bookingId = int.Parse(postResponse["bookingid"].ToString()!);

            // GET /booking/{id}
            var booking = await _client.GetAsync<Booking>($"booking/{bookingId}");
            AllureApi.Step("Assert booking record retrieval and its fields", () =>
            {
                Assert.That(booking, Is.Not.Null, "Booking details not found for fresh booking");
                Assert.That(booking.firstname, Is.EqualTo(newBooking.firstname));
                Assert.That(booking.lastname, Is.EqualTo(newBooking.lastname));
                Assert.That(booking.totalprice, Is.EqualTo(newBooking.totalprice));
                Assert.That(booking.depositpaid, Is.EqualTo(newBooking.depositpaid));
                Assert.That(booking.bookingdates.checkin, Is.EqualTo(newBooking.bookingdates.checkin));
                Assert.That(booking.bookingdates.checkout, Is.EqualTo(newBooking.bookingdates.checkout));
                Assert.That(booking.additionalneeds, Is.EqualTo(newBooking.additionalneeds));
            });
        }
    }
}
