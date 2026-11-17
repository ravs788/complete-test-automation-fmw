using Allure.Net.Commons.Attributes;
using Core.Reporting;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[AllureParentSuite("API")]
[AllureSuite("Restful Booker")]
[AllureSubSuite("Health check")]
[AllureTag("api", "restful-booker", "health", "smoke", "regression")]
[Trait("Category", "smoke")]
[Trait("Category", "regression")]
public class BookingApiHealthCheckTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task Ping_Should_Return201() => RunAsync(async () =>
    {
        var response = await Client.SendRawAsync("GET", "ping");

        Report.Step("Assert API health check via /ping returns 201", () =>
            Assert.Equal(201, response.Status));
    });
}
