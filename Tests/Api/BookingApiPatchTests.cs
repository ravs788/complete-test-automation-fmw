using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.Models.RestfulBooker;
using Core.TestData.RestfulBooker;
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
    public Task Patch_Firstname_Lastname_Then_Verify() => RunAsync(async () =>
    {
        var original = BookingMother.Standard();
        var created = await CreateBookingAsync(original);
        AllureApi.Step("Assert booking created for patch test", () =>
        {
            Assert.True(created.BookingId > 0, "Post response missing id");
        });

        var auth = await GetAuthHeadersAsync();
        var patch = new BookingPatch { FirstName = "Patched", LastName = "Name" };
        var patched = await Client.PatchAsync<Booking>($"booking/{created.BookingId}", patch, auth);

        AllureApi.Step("Assert patched fields changed and the other fields are unchanged", () =>
            Assert.Equal(patch.ApplyTo(original), patched));
    });
}
