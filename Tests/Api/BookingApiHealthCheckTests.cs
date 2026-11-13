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

[AllureSuite("Booking Health Check API")]
[AllureTag("api", "health", "smoke", "regression")]
[Trait("Category", "smoke")]
[Trait("Category", "regression")]
public class BookingApiHealthCheckTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task BookingApi_Ping_Should_Return_201() => RunAsync(async () =>
    {
        var response = await Client.SendRawAsync("GET", "ping");

        AllureApi.Step("Assert API health check via /ping returns 201", () =>
            Assert.Equal(201, response.Status));
    });
}
