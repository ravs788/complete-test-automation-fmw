using Core.Reporting;
using Domain.Models.RestfulBooker;
using Domain.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[ReportSuite("API", "Restful Booker", "Get booking")]
[ReportTags("api", "restful-booker", "get", "regression")]
[Trait("Category", "regression")]
public class BookingApiGetTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task BookingIds_Should_BeListed() => RunAsync(async () =>
    {
        // Arrange: the fixture supplies the configured API client.
        // Act
        var bookingIds = await Client.GetAsync<List<BookingId>>("booking");

        // Assert
        Report.Step("Assert that bookingIds are returned", () =>
        {
            Assert.NotNull(bookingIds);
            Assert.NotEmpty(bookingIds);
        });
    });

    [Fact]
    public Task Booking_Should_BeReturnedById() => RunAsync(async () =>
    {
        // Arrange
        var newBooking = BookingMother.Standard();

        var created = await CreateBookingAsync(newBooking);

        // Assert: confirm the arranged booking exists.
        Report.Step("Assert booking created for get-by-id test", () =>
        {
            Assert.True(created.BookingId > 0, "Post response does not include a bookingid");
        });

        // Act
        var booking = await Client.GetAsync<Booking>($"booking/{created.BookingId}");

        // Assert
        Report.Step("Assert booking record retrieval and its fields", () =>
            Assert.Equal(newBooking, booking));
    });
}
