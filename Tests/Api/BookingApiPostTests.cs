using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[AllureParentSuite("API")]
[AllureSuite("Restful Booker")]
[AllureSubSuite("Create booking")]
[AllureTag("api", "restful-booker", "post", "regression")]
[Trait("Category", "regression")]
public class BookingApiPostTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task NewBooking_Should_BeCreated() => RunAsync(async () =>
    {
        var newBooking = BookingMother.Standard(); // unique last name, so the shared demo API never sees a duplicate

        var created = await CreateBookingAsync(newBooking);

        AllureApi.Step("Assert booking creation response", () =>
        {
            Assert.True(created.BookingId > 0, "Response missing bookingid");
            Assert.Equal(newBooking, created.Booking);
        });
    });
}
