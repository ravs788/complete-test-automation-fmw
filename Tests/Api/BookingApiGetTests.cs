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

[AllureSuite("Booking Get API")]
[AllureTag("api", "get", "regression")]
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
        var newBooking = TestDataLoader.Instance.Load<Booking>("BookingApiGetTests/ById.json");

        var created = await Client.PostAsync<CreatedBooking>("booking", newBooking);
        AllureApi.Step("Assert booking created for get-by-id test", () =>
        {
            Assert.NotNull(created);
            Assert.True(created.bookingid > 0, "Post response does not include a bookingid");
        });

        var booking = await Client.GetAsync<Booking>($"booking/{created!.bookingid}");
        AllureApi.Step("Assert booking record retrieval and its fields", () =>
            Assert.Equal(newBooking, booking));
    });
}
