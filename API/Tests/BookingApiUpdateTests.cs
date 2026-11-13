using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using API.Utilities;
using API.Models;
using Core.Utilities;
using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Xunit;
using Xunit.Sdk;

namespace API.Tests
{
    [AllureSuite("Booking Update API")]
    [AllureTag("api", "put", "regression")]
    [Trait("Category", "regression")]
    public class BookingApiUpdateTests : BaseApiTest
    {



        [Fact]
        public Task Update_Booking_Then_Verify() => RunAsync(async () =>
        {
            // 1. Create a booking
            var original = TestDataLoader.Instance.Load<Booking>(
                "BookingApiUpdateTests/Original.json"
            );
            var postResp = await _client.PostAsync<Booking, Dictionary<string, object>>("booking", original);
            AllureApi.Step("Assert original booking creation response", () =>
            {
                try
                {
                    Assert.NotNull(postResp);
                    Assert.True(postResp.ContainsKey("bookingid"), "Post response missing bookingid");
                    Logger.Info("PASSED: Assert original booking creation response (bookingid present)");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert original booking creation response - {ex.Message}");
                    throw;
                }
            });
            int bookingId = int.Parse(postResp["bookingid"].ToString()!);

            // 2. Auth
            var token = await AuthHelper.GetAuthTokenAsync();

            // 3. Update via PUT
            var updated = TestDataLoader.Instance.Load<Booking>(
                "BookingApiUpdateTests/Update.json"
            );
            var updateResponse = await _client.PutAsync<Booking, Booking>($"booking/{bookingId}", updated, token);

            AllureApi.Step("Assert updated booking fields via PUT", () =>
            {
                try
                {
                    Assert.NotNull(updateResponse);
                    Assert.Equal("Updated", updateResponse.firstname);
                    Assert.Equal("Person", updateResponse.lastname);
                    Assert.Equal(222, updateResponse.totalprice);
                    Assert.True(updateResponse.depositpaid);
                    Assert.Equal("2025-09-08", updateResponse.bookingdates.checkin);
                    Assert.Equal("2025-09-12", updateResponse.bookingdates.checkout);
                    Assert.Equal("Dinner", updateResponse.additionalneeds);
                    Logger.Info("PASSED: Assert updated booking fields via PUT");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert updated booking fields via PUT - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
