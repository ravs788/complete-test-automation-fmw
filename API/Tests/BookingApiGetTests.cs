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
    [AllureTag("api", "get", "regression")]
    public class BookingApiGetTests : BaseApiTest
    {


        [Test]
        public async Task Get_All_Booking_Ids()
        {
            // /booking returns array of objects with bookingid fields
            var bookingIds = await _client.GetAsync<List<Dictionary<string, int>>>("booking");
            AllureApi.Step("Assert that bookingIds are returned", () =>
            {
                try
                {
                    Assert.That(bookingIds, Is.Not.Null.And.Not.Empty, "Booking IDs should be returned");
                    Logger.Info("PASSED: Assert that bookingIds are returned (Not.Null/Not.Empty)");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert that bookingIds are returned - {ex.Message}");
                    throw;
                }
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
                try
                {
                    Assert.That(postResponse, Is.Not.Null, "Booking creation failed");
                    Assert.That(postResponse.ContainsKey("bookingid"), "Post response does not include bookingid");
                    Logger.Info("PASSED: Assert booking created for get-by-id test");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert booking created for get-by-id test - {ex.Message}");
                    throw;
                }
            });

            int bookingId = int.Parse(postResponse["bookingid"].ToString()!);

            // GET /booking/{id}
            var booking = await _client.GetAsync<Booking>($"booking/{bookingId}");
            AllureApi.Step("Assert booking record retrieval and its fields", () =>
            {
                try
                {
                    Assert.That(booking, Is.Not.Null, "Booking details not found for fresh booking");
                    Assert.That(booking.firstname, Is.EqualTo(newBooking.firstname));
                    Assert.That(booking.lastname, Is.EqualTo(newBooking.lastname));
                    Assert.That(booking.totalprice, Is.EqualTo(newBooking.totalprice));
                    Assert.That(booking.depositpaid, Is.EqualTo(newBooking.depositpaid));
                    Assert.That(booking.bookingdates.checkin, Is.EqualTo(newBooking.bookingdates.checkin));
                    Assert.That(booking.bookingdates.checkout, Is.EqualTo(newBooking.bookingdates.checkout));
                    Assert.That(booking.additionalneeds, Is.EqualTo(newBooking.additionalneeds));
                    Logger.Info("PASSED: Assert booking record retrieval and all checked fields match expected values.");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert booking record retrieval and fields - {ex.Message}");
                    throw;
                }
            });
        }
    }
}
