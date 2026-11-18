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

    [Theory]
    [InlineData("api", "items/echo", "/api/items/echo")]
    [InlineData("api/", "items/echo", "/api/items/echo")]
    [InlineData("api", "/root/echo", "/root/echo")]
    [InlineData("api/", "/root/echo", "/root/echo")]
    [InlineData("api", "../echo", "/echo")]
    [InlineData("api/", "../echo", "/echo")]
    public async Task BaseUrl_Should_BeADirectory_WithStandardRelativeResolution(string basePath, string endpoint, string expected)
    {
        await using var transport = await TransportAsync(_server.BaseUrl + basePath);
        var response = await transport.SendAsync(new HttpRequestData("GET", endpoint, NoHeaders));
        Assert.Equal(expected, Echo(response)["path"]);
    }

    [Theory]
    [InlineData("api")]
    [InlineData("api/")]
    public async Task AbsoluteUrl_Should_OverrideTheBaseHostAndPath(string basePath)
    {
        await using var transport = await TransportAsync("http://127.0.0.1:1/" + basePath);
        var response = await transport.SendAsync(new HttpRequestData("GET", _server.BaseUrl + "absolute/echo", NoHeaders));
        Assert.Equal("/absolute/echo", Echo(response)["path"]);
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

    [Fact]
    public async Task PreCancelledRequest_Should_NotReachServer_And_TransportRemainsUsable()
    {
        await using var transport = await TransportAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            transport.SendAsync(new HttpRequestData("POST", "cancelled", NoHeaders), cancellation.Token));
        Assert.Equal(200, (await transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders))).Status);
        Assert.DoesNotContain("/cancelled", _server.RequestPaths);
    }

    [Fact]
    public async Task CancellingOneRequest_Should_NotAbortConcurrentRequests_OrPoisonTransport()
    {
        await using var transport = await TransportAsync();
        using var cancellation = new CancellationTokenSource();
        var delayed = transport.SendAsync(new HttpRequestData("GET", "delay", NoHeaders), cancellation.Token);
        try
        {
            await _server.DelayedRequestReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var peer = transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders));
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => delayed.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(200, (await peer.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            Assert.Equal(200, (await transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders))).Status);
        }
        finally { _server.ReleaseDelayedResponse(); }
    }

    [Fact]
    public async Task Disposal_Should_CancelAnActiveRequest()
    {
        var transport = await TransportAsync();
        var delayed = transport.SendAsync(new HttpRequestData("GET", "delay", NoHeaders));
        try
        {
            await _server.DelayedRequestReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await transport.DisposeAsync();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => delayed.WaitAsync(TimeSpan.FromSeconds(5)));
        }
        finally
        {
            _server.ReleaseDelayedResponse();
            await transport.DisposeAsync();
        }
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
