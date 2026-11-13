using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.Models.RestfulBooker;
using Core.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[AllureParentSuite("API")]
[AllureSuite("Restful Booker")]
[AllureSubSuite("Update booking")]
[AllureTag("api", "restful-booker", "put", "regression")]
[Trait("Category", "regression")]
public class BookingApiUpdateTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task UpdatedBooking_Should_ReplaceAllFields() => RunAsync(async () =>
    {
        var original = BookingMother.Standard();
        var created = await CreateBookingAsync(original);
        AllureApi.Step("Assert original booking creation response", () =>
        {
            Assert.True(created.BookingId > 0, "Post response missing bookingid");
        });

        var auth = await GetAuthHeadersAsync();
        var updated = BookingMother.FullyChanged();
        var updateResponse = await Client.PutAsync<Booking>($"booking/{created.BookingId}", updated, auth);

        AllureApi.Step("Assert updated booking fields via PUT", () =>
            Assert.Equal(updated, updateResponse));
    });
}
