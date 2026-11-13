using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.Models.RestfulBooker;
using Core.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[AllureParentSuite("API")]
[AllureSuite("Restful Booker")]
[AllureSubSuite("Get booking")]
[AllureTag("api", "restful-booker", "get", "regression")]
[Trait("Category", "regression")]
public class BookingApiGetTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task Get_All_Booking_Ids() => RunAsync(async () =>
    {
        var bookingIds = await Client.GetAsync<List<BookingId>>("booking");

        AllureApi.Step("Assert that bookingIds are returned", () =>
        {
            Assert.NotNull(bookingIds);
            Assert.NotEmpty(bookingIds);
        });
    });

    [Fact]
    public Task Get_Booking_By_Id() => RunAsync(async () =>
    {
        var newBooking = BookingMother.Standard();

        var created = await Client.PostAsync<CreatedBooking>("booking", newBooking);
        AllureApi.Step("Assert booking created for get-by-id test", () =>
        {
            Assert.NotNull(created);
            Assert.True(created.BookingId > 0, "Post response does not include a bookingid");
        });

        var booking = await Client.GetAsync<Booking>($"booking/{created!.BookingId}");
        AllureApi.Step("Assert booking record retrieval and its fields", () =>
            Assert.Equal(newBooking, booking));
    });
}
