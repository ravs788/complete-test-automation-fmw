using Core.Reporting;
using Domain.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[ReportSuite("API", "Restful Booker", "Delete booking")]
[ReportTags("api", "restful-booker", "delete", "regression")]
[Trait("Category", "regression")]
public class BookingApiDeleteTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task DeletedBooking_Should_ReturnNotFound() => RunAsync(async () =>
    {
        // Arrange
        var booking = BookingMother.Standard();
        var created = await CreateBookingAsync(booking);

        // Assert: confirm the arranged booking exists.
        Report.Step("Assert booking creation for delete test", () =>
        {
            Assert.True(created.BookingId > 0, "Booking creation missing id");
        });

        // Arrange: obtain authorization for deletion.
        var auth = await GetAuthHeadersAsync();

        // Act
        var status = await Client.DeleteAsync($"booking/{created.BookingId}", auth);

        // Assert
        Report.Step("Assert delete booking response", () =>
            Assert.Equal(201, status)); // Restful Booker answers DELETE with 201 Created

        // Act: retrieve the deleted booking.
        var afterDelete = await Client.SendRawAsync("GET", $"booking/{created.BookingId}");

        // Assert: the booking is no longer available.
        Report.Step("Assert deleted booking is not found (404)", () =>
            Assert.Equal(404, afterDelete.Status));
    });
}
