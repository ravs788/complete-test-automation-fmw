using Core.Reporting;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.Api;

[ReportSuite("API", "Restful Booker", "Health check")]
[ReportTags("api", "restful-booker", "health", "smoke", "regression")]
[Trait("Category", "smoke")]
[Trait("Category", "regression")]
public class BookingApiHealthCheckTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public Task Ping_Should_Return201() => RunAsync(async () =>
    {
        // Arrange: the fixture supplies the configured API client.
        // Act
        var response = await Client.SendRawAsync("GET", "ping");

        // Assert
        Report.Step("Assert API health check via /ping returns 201", () =>
            Assert.Equal(201, response.Status));
    });
}
