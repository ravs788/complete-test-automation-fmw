using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[AllureParentSuite("API")]
[AllureSuite("Restful Booker")]
[AllureSubSuite("Delete booking")]
[AllureTag("api", "restful-booker", "delete", "regression")]
[Trait("Category", "regression")]
public class BookingApiDeleteTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task Delete_Booking_Then_Verify_Should_Fail_To_Get() => RunAsync(async () =>
    {
        var booking = BookingMother.Standard();
        var created = await CreateBookingAsync(booking);
        AllureApi.Step("Assert booking creation for delete test", () =>
        {
            Assert.True(created.BookingId > 0, "Booking creation missing id");
        });

        var auth = await GetAuthHeadersAsync();
        var status = await Client.DeleteAsync($"booking/{created.BookingId}", auth);
        AllureApi.Step("Assert delete booking response", () =>
            Assert.Equal(201, status)); // Restful Booker answers DELETE with 201 Created

        var afterDelete = await Client.SendRawAsync("GET", $"booking/{created.BookingId}");
        AllureApi.Step("Assert deleted booking is not found (404)", () =>
            Assert.Equal(404, afterDelete.Status));
    });
}
