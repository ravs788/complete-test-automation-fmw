using Core.Reporting;
using Domain.Models.RestfulBooker;
using Domain.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[ReportSuite("API", "Restful Booker", "Patch booking")]
[ReportTags("api", "restful-booker", "patch", "regression")]
[Trait("Category", "regression")]
public class BookingApiPatchTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task PatchedBooking_Should_ChangeOnlyPatchedFields() => RunAsync(async () =>
    {
        // Arrange
        var original = BookingMother.Standard();
        var created = await CreateBookingAsync(original);

        // Assert: confirm the arranged booking exists.
        Report.Step("Assert booking created for patch test", () =>
        {
            Assert.True(created.BookingId > 0, "Post response missing id");
        });

        // Arrange: obtain authorization and prepare the partial update.
        var auth = await GetAuthHeadersAsync();
        var patch = new BookingPatch { FirstName = "Patched", LastName = "Name" };

        // Act
        var patched = await Client.PatchAsync<Booking>($"booking/{created.BookingId}", patch, auth);

        // Assert
        Report.Step("Assert patched fields changed and the other fields are unchanged", () =>
            Assert.Equal(patch.ApplyTo(original), patched));

        // Act: retrieve the booking after the update.
        var persisted = await Client.GetAsync<Booking>($"booking/{created.BookingId}");

        // Assert: the changes were persisted.
        Report.Step("Assert booking changes were persisted", () =>
            Assert.Equal(patch.ApplyTo(original), persisted));
    });
}
