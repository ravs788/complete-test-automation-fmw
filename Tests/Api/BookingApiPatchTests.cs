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

[AllureSuite("Booking Patch API")]
[AllureTag("api", "patch", "regression")]
[Trait("Category", "regression")]
public class BookingApiPatchTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task Patch_Firstname_Lastname_Then_Verify() => RunAsync(async () =>
    {
        var original = TestDataLoader.Instance.Load<Booking>("BookingApiPatchTests/Original.json");
        var created = await Client.PostAsync<CreatedBooking>("booking", original);
        AllureApi.Step("Assert booking created for patch test", () =>
        {
            Assert.NotNull(created);
            Assert.True(created.bookingid > 0, "Post response missing id");
        });

        var token = await GetAuthTokenAsync();
        var patchData = TestDataLoader.Instance.Load<Dictionary<string, string>>("BookingApiPatchTests/Patch.json");
        var patched = await Client.PatchAsync<Booking>($"booking/{created!.bookingid}", patchData, token);

        AllureApi.Step("Assert firstname and lastname were patched, other fields unchanged", () =>
        {
            var expected = original with { firstname = patchData["firstname"], lastname = patchData["lastname"] };
            Assert.Equal(expected, patched);
        });
    });
}
