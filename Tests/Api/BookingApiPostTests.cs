namespace Tests.Api;

using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.Api;
using Core.Api.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;
using Core.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

[AllureSuite("Booking Post API")]
[AllureTag("api", "post", "regression")]
[Trait("Category", "regression")]
public class BookingApiPostTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task Create_New_Booking_Should_Succeed() => RunAsync(async () =>
    {
        var newBooking = TestDataLoader.Instance.Load<Booking>("BookingApiPostTests/HappyPath.json");
        // Randomize a key property to avoid demo API duplication/spam rejection
        newBooking = newBooking with { lastname = $"{newBooking.lastname}_{System.DateTime.UtcNow.Ticks}" };

        var created = await Client.PostAsync<CreatedBooking>("booking", newBooking);

        AllureApi.Step("Assert booking creation response", () =>
        {
            Assert.NotNull(created);
            Assert.True(created.bookingid > 0, "Response missing bookingid");
            Assert.Equal(newBooking, created.booking);
        });
    });
}
