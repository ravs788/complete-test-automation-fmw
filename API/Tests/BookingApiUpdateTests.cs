using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using API.Utilities;
using API.Models;
using Core.Utilities;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace API.Tests
{
    [AllureNUnit]
    [AllureSuite("Booking Update API")]
    [AllureTag("api", "put", "regression")]
    public class BookingApiUpdateTests : BaseApiTest
    {



        [Test]
        public async Task Update_Booking_Then_Verify()
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
                    Assert.That(postResp, Is.Not.Null, "Post response should not be null");
                    Assert.That(postResp.ContainsKey("bookingid"), "Post response missing bookingid");
                    Logger.Info("PASSED: Assert original booking creation response (bookingid present)");
                }
                catch (AssertionException ex)
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
                    Assert.That(updateResponse, Is.Not.Null);
                    Assert.That(updateResponse.firstname, Is.EqualTo("Updated"));
                    Assert.That(updateResponse.lastname, Is.EqualTo("Person"));
                    Assert.That(updateResponse.totalprice, Is.EqualTo(222));
                    Assert.That(updateResponse.depositpaid, Is.True);
                    Assert.That(updateResponse.bookingdates.checkin, Is.EqualTo("2025-09-08"));
                    Assert.That(updateResponse.bookingdates.checkout, Is.EqualTo("2025-09-12"));
                    Assert.That(updateResponse.additionalneeds, Is.EqualTo("Dinner"));
                    Logger.Info("PASSED: Assert updated booking fields via PUT");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert updated booking fields via PUT - {ex.Message}");
                    throw;
                }
            });
        }
    }
}
