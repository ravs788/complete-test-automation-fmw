using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utilities;
using API.Models;
using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Xunit;
using Xunit.Sdk;

namespace API.Tests
{
    [AllureSuite("Booking Post API")]
    [AllureTag("api", "post", "regression")]
    [Trait("Category", "regression")]
    public class BookingApiPostTests : BaseApiTest
    {


        [Fact]
        public Task Create_New_Booking_Should_Succeed() => RunAsync(async () =>
        {
            var newBooking = Core.Utilities.TestDataLoader.Instance.Load<API.Models.Booking>(
                "BookingApiPostTests/HappyPath.json"
            );
            // Randomize a key property to avoid demo API duplication/spam rejection
            newBooking = newBooking with { lastname = $"{newBooking.lastname}_{System.DateTime.UtcNow.Ticks}" };
            var postResponse = await _client.PostAsync<Booking, Dictionary<string, object>>("booking", newBooking);
            AllureApi.Step("Assert booking creation response", () =>
            {
                try
                {
                    Assert.NotNull(postResponse);
                    Assert.True(postResponse.ContainsKey("bookingid"), "Response missing bookingid");
                    Logger.Info("PASSED: Assert booking creation response");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert booking creation response - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
