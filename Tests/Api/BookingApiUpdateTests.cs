using Core.Reporting;
using Domain.Models.RestfulBooker;
using Domain.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[ReportSuite("API", "Restful Booker", "Update booking")]
[ReportTags("api", "restful-booker", "put", "regression")]
[Trait("Category", "regression")]
public class BookingApiUpdateTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task UpdatedBooking_Should_ReplaceAllFields() => RunAsync(async () =>
    {
        // Arrange
        var original = BookingMother.Standard();
        var created = await CreateBookingAsync(original);

        // Assert: confirm the arranged booking exists.
        Report.Step("Assert original booking creation response", () =>
        {
            Assert.True(created.BookingId > 0, "Post response missing bookingid");
        });

        // Arrange: obtain authorization and prepare the replacement values.
        var auth = await GetAuthHeadersAsync();
        var updated = BookingMother.FullyChanged();

        // Act
        var updateResponse = await Client.PutAsync<Booking>($"booking/{created.BookingId}", updated, auth);

        // Assert
        Report.Step("Assert updated booking fields via PUT", () =>
            Assert.Equal(updated, updateResponse));

        // Act: retrieve the booking after the update.
        var persisted = await Client.GetAsync<Booking>($"booking/{created.BookingId}");

        // Assert: the changes were persisted.
        Report.Step("Assert booking changes were persisted", () =>
            Assert.Equal(updated, persisted));
    });
}
