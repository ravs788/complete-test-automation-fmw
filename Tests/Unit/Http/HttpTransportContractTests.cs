using System.Text.Json;
using Core.Http;
using Core.Http.Playwright;

namespace Tests.Unit.Http;

/// <summary>
/// The behaviour every <see cref="IHttpTransport"/> must have (R41, R46). Each transport's test class inherits these
/// tests, so all of them are held to the same contract.
/// </summary>
public abstract class HttpTransportContractTests : IDisposable
{
    private static readonly IReadOnlyDictionary<string, string> Accept = new Dictionary<string, string> { ["Accept"] = "application/json" };
    private static readonly IReadOnlyDictionary<string, string> NoHeaders = new Dictionary<string, string>();

    private readonly LoopbackHttpServer _server = new();

    protected abstract IHttpTransportFactory CreateFactory();

    private Task<IHttpTransport> TransportAsync(string? baseUrl = null) => CreateFactory().CreateAsync(baseUrl ?? _server.BaseUrl, Accept);

    private static Dictionary<string, string?> Echo(HttpResponseData response) =>
        JsonSerializer.Deserialize<Dictionary<string, string?>>(response.Body)!;

    [Fact]
    public async Task Get_Should_ReturnStatus_Body_And_Headers()
    {
        await using var transport = await TransportAsync();

        var response = await transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders));

        Assert.Equal(200, response.Status);
        Assert.True(response.Ok);
        Assert.Equal("GET", Echo(response)["method"]);
        Assert.Equal("application/json", Echo(response)["accept"]);
        Assert.Equal("yes", response.Headers["x-test"]);
    }

    [Fact]
    public async Task Post_Should_SendBody_ContentType_And_RequestHeaders()
    {
        await using var transport = await TransportAsync();
        var headers = new Dictionary<string, string> { ["Cookie"] = "token=abc123" };

        var response = await transport.SendAsync(new HttpRequestData("POST", "echo", headers, "{\"a\":1}", "application/json"));

        var echo = Echo(response);
        Assert.Equal("POST", echo["method"]);
        Assert.Equal("{\"a\":1}", echo["body"]);
        Assert.StartsWith("application/json", echo["contentType"]);
        Assert.Equal("token=abc123", echo["cookie"]);
    }

    [Fact]
    public async Task RelativeUrls_Should_ResolveAgainst_TheBaseUrl()
    {
        await using var transport = await TransportAsync(_server.BaseUrl + "api/");

        var response = await transport.SendAsync(new HttpRequestData("GET", "items/echo", NoHeaders));

        Assert.Equal("/api/items/echo", Echo(response)["path"]);
    }

    [Fact]
    public async Task NonSuccess_Should_BeReturned_NotThrown()
    {
        await using var transport = await TransportAsync();

        var response = await transport.SendAsync(new HttpRequestData("GET", "missing", NoHeaders));

        Assert.Equal(404, response.Status);
        Assert.False(response.Ok);
        Assert.Equal("missing", response.Text);
    }

    [Fact]
    public async Task NoResponse_Should_Throw_HttpRequestException()
    {
        await using var transport = await TransportAsync($"http://127.0.0.1:{LoopbackHttpServer.FreePort()}/");

        await Assert.ThrowsAsync<HttpRequestException>(() => transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders)));
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class HttpClientTransportContractTests : HttpTransportContractTests
{
    protected override IHttpTransportFactory CreateFactory() => new HttpClientTransportFactory();
}

public sealed class PlaywrightHttpTransportContractTests : HttpTransportContractTests
{
    protected override IHttpTransportFactory CreateFactory() => new PlaywrightHttpTransportFactory();
}
