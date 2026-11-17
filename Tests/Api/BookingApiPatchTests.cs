using Allure.Net.Commons.Attributes;
using Core.Reporting;
using Domain.Models.RestfulBooker;
using Domain.TestData.RestfulBooker;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[AllureParentSuite("API")]
[AllureSuite("Restful Booker")]
[AllureSubSuite("Patch booking")]
[AllureTag("api", "restful-booker", "patch", "regression")]
[Trait("Category", "regression")]
public class BookingApiPatchTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task PatchedBooking_Should_ChangeOnlyPatchedFields() => RunAsync(async () =>
    {
        var original = BookingMother.Standard();
        var created = await CreateBookingAsync(original);
        Report.Step("Assert booking created for patch test", () =>
        {
            Assert.True(created.BookingId > 0, "Post response missing id");
        });

        var auth = await GetAuthHeadersAsync();
        var patch = new BookingPatch { FirstName = "Patched", LastName = "Name" };
        var patched = await Client.PatchAsync<Booking>($"booking/{created.BookingId}", patch, auth);

        Report.Step("Assert patched fields changed and the other fields are unchanged", () =>
            Assert.Equal(patch.ApplyTo(original), patched));

        var persisted = await Client.GetAsync<Booking>($"booking/{created.BookingId}");
        Report.Step("Assert booking changes were persisted", () =>
            Assert.Equal(patch.ApplyTo(original), persisted));
    });
}
