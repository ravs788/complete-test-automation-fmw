using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utilities;
using API.Models;
using Core.Utilities;
using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Xunit;
using Xunit.Sdk;

namespace API.Tests
{
    [AllureSuite("Booking Get API")]
    [AllureTag("api", "get", "regression")]
    [Trait("Category", "regression")]
    public class BookingApiGetTests : BaseApiTest
    {


        [Fact]
        public Task Get_All_Booking_Ids() => RunAsync(async () =>
        {
            // /booking returns array of objects with bookingid fields
            var bookingIds = await _client.GetAsync<List<Dictionary<string, int>>>("booking");
            AllureApi.Step("Assert that bookingIds are returned", () =>
            {
                try
                {
                    Assert.NotNull(bookingIds);
                    Assert.NotEmpty(bookingIds);
                    Logger.Info("PASSED: Assert that bookingIds are returned (Not.Null/Not.Empty)");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert that bookingIds are returned - {ex.Message}");
                    throw;
                }
            });
        });

        [Fact]
        public Task Get_Booking_By_Id() => RunAsync(async () =>
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
                    Assert.NotNull(postResponse);
                    Assert.True(postResponse.ContainsKey("bookingid"), "Post response does not include bookingid");
                    Logger.Info("PASSED: Assert booking created for get-by-id test");
                }
                catch (XunitException ex)
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
                    Assert.NotNull(booking);
                    Assert.Equal(newBooking.firstname, booking.firstname);
                    Assert.Equal(newBooking.lastname, booking.lastname);
                    Assert.Equal(newBooking.totalprice, booking.totalprice);
                    Assert.Equal(newBooking.depositpaid, booking.depositpaid);
                    Assert.Equal(newBooking.bookingdates.checkin, booking.bookingdates.checkin);
                    Assert.Equal(newBooking.bookingdates.checkout, booking.bookingdates.checkout);
                    Assert.Equal(newBooking.additionalneeds, booking.additionalneeds);
                    Logger.Info("PASSED: Assert booking record retrieval and all checked fields match expected values.");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert booking record retrieval and fields - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
