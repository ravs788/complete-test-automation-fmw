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

[AllureSuite("Booking Update API")]
[AllureTag("api", "put", "regression")]
[Trait("Category", "regression")]
public class BookingApiUpdateTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task Update_Booking_Then_Verify() => RunAsync(async () =>
    {
        var original = TestDataLoader.Instance.Load<Booking>("BookingApiUpdateTests/Original.json");
        var created = await Client.PostAsync<CreatedBooking>("booking", original);
        AllureApi.Step("Assert original booking creation response", () =>
        {
            Assert.NotNull(created);
            Assert.True(created.bookingid > 0, "Post response missing bookingid");
        });

        var token = await GetAuthTokenAsync();
        var updated = TestDataLoader.Instance.Load<Booking>("BookingApiUpdateTests/Update.json");
        var updateResponse = await Client.PutAsync<Booking>($"booking/{created!.bookingid}", updated, token);

        AllureApi.Step("Assert updated booking fields via PUT", () =>
            Assert.Equal(updated, updateResponse));
    });
}
