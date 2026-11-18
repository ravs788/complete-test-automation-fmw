using Core.Reporting;
using Domain.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[ReportSuite("API", "Restful Booker", "Create booking")]
[ReportTags("api", "restful-booker", "post", "regression")]
[Trait("Category", "regression")]
public class BookingApiPostTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task NewBooking_Should_BeCreated() => RunAsync(async () =>
    {
        // Arrange
        var newBooking = BookingMother.Standard(); // unique last name, so the shared demo API never sees a duplicate

        // Act
        var created = await CreateBookingAsync(newBooking);

        // Assert
        Report.Step("Assert booking creation response", () =>
        {
            Assert.True(created.BookingId > 0, "Response missing bookingid");
            Assert.Equal(newBooking, created.Booking);
        });
    });
}
