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
        // Arrange
        await using var transport = await TransportAsync();

        // Act
        var response = await transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders));

        // Assert
        Assert.Equal(200, response.Status);
        Assert.True(response.Ok);
        Assert.Equal("GET", Echo(response)["method"]);
        Assert.Equal("application/json", Echo(response)["accept"]);
        Assert.Equal("yes", response.Headers["x-test"]);
    }

    [Fact]
    public async Task Post_Should_SendBody_ContentType_And_RequestHeaders()
    {
        // Arrange
        await using var transport = await TransportAsync();
        var headers = new Dictionary<string, string> { ["Cookie"] = "token=abc123" };

        // Act
        var response = await transport.SendAsync(new HttpRequestData("POST", "echo", headers, "{\"a\":1}", "application/json"));

        var echo = Echo(response);

        // Assert
        Assert.Equal("POST", echo["method"]);
        Assert.Equal("{\"a\":1}", echo["body"]);
        Assert.StartsWith("application/json", echo["contentType"]);
        Assert.Equal("token=abc123", echo["cookie"]);
    }

    [Fact]
    public async Task RelativeUrls_Should_ResolveAgainst_TheBaseUrl()
    {
        // Arrange
        await using var transport = await TransportAsync(_server.BaseUrl + "api/");

        // Act
        var response = await transport.SendAsync(new HttpRequestData("GET", "items/echo", NoHeaders));

        // Assert
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
        // Arrange
        await using var transport = await TransportAsync(_server.BaseUrl + basePath);

        // Act
        var response = await transport.SendAsync(new HttpRequestData("GET", endpoint, NoHeaders));

        // Assert
        Assert.Equal(expected, Echo(response)["path"]);
    }

    [Theory]
    [InlineData("api")]
    [InlineData("api/")]
    public async Task AbsoluteUrl_Should_OverrideTheBaseHostAndPath(string basePath)
    {
        // Arrange
        await using var transport = await TransportAsync("http://127.0.0.1:1/" + basePath);

        // Act
        var response = await transport.SendAsync(new HttpRequestData("GET", _server.BaseUrl + "absolute/echo", NoHeaders));

        // Assert
        Assert.Equal("/absolute/echo", Echo(response)["path"]);
    }

    [Theory]
    [InlineData(301, "GET")]
    [InlineData(302, "GET")]
    [InlineData(303, "POST")]
    [InlineData(307, "POST")]
    [InlineData(308, "PUT")]
    public async Task CrossOriginRedirect_Should_NotForwardCredentialsOrBody(int status, string method)
    {
        // Arrange
        using var destination = new LoopbackHttpServer();
        _server.Redirects["/redirect"] = (status, destination.BaseUrl + "echo");
        var secrets = new Dictionary<string, string>
        {
            ["Cookie"] = "token=synthetic-cookie",
            ["Authorization"] = "Bearer synthetic-auth",
            ["X-Api-Key"] = "synthetic-key"
        };
        await using var transport = await CreateFactory().CreateAsync(_server.BaseUrl, secrets);

        // Act
        var response = await transport.SendAsync(new HttpRequestData(method, "redirect", secrets, "synthetic-body"));

        // Assert
        Assert.Equal(status, response.Status);
        Assert.Equal(destination.BaseUrl + "echo", response.Headers["location"]);
        Assert.Empty(destination.RequestPaths);
        Assert.Equal(["/redirect"], _server.RequestPaths.ToArray());
    }

    [Theory]
    [InlineData("/echo")]
    [InlineData("/redirect")]
    public async Task SameOriginRedirect_OrLoop_Should_ReturnTheFirstResponse(string target)
    {
        // Arrange
        _server.Redirects["/redirect"] = (302, target);
        await using var transport = await TransportAsync();

        // Act
        var response = await transport.SendAsync(new HttpRequestData("GET", "redirect", NoHeaders));

        // Assert
        Assert.Equal(302, response.Status);
        Assert.Equal(["/redirect"], _server.RequestPaths.ToArray());
    }

    [Fact]
    public async Task NonSuccess_Should_BeReturned_NotThrown()
    {
        // Arrange
        await using var transport = await TransportAsync();

        // Act
        var response = await transport.SendAsync(new HttpRequestData("GET", "missing", NoHeaders));

        // Assert
        Assert.Equal(404, response.Status);
        Assert.False(response.Ok);
        Assert.Equal("missing", response.Text);
    }

    [Fact]
    public async Task NoResponse_Should_Throw_HttpRequestException()
    {
        // Arrange
        await using var transport = await TransportAsync($"http://127.0.0.1:{LoopbackHttpServer.FreePort()}/");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders)));
    }

    [Fact]
    public async Task PreCancelledRequest_Should_NotReachServer_And_TransportRemainsUsable()
    {
        // Arrange
        await using var transport = await TransportAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            transport.SendAsync(new HttpRequestData("POST", "cancelled", NoHeaders), cancellation.Token));

        // Act & Assert: the transport still serves a subsequent request.
        Assert.Equal(200, (await transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders))).Status);

        // Assert: the pre-cancelled request never reached the server.
        Assert.DoesNotContain("/cancelled", _server.RequestPaths);
    }

    [Fact]
    public async Task CancellingOneRequest_Should_NotAbortConcurrentRequests_OrPoisonTransport()
    {
        // Arrange
        await using var transport = await TransportAsync();
        using var cancellation = new CancellationTokenSource();

        // Arrange: start a request that can be cancelled while a peer request proceeds.
        var delayed = transport.SendAsync(new HttpRequestData("GET", "delay", NoHeaders), cancellation.Token);
        try
        {
            await _server.DelayedRequestReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var peer = transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders));

            // Act: cancel only the delayed request.
            cancellation.Cancel();

            // Assert: cancellation affects only the selected request; the transport remains usable.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => delayed.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(200, (await peer.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            Assert.Equal(200, (await transport.SendAsync(new HttpRequestData("GET", "echo", NoHeaders))).Status);
        }

        // Cleanup: release the delayed server response.
        finally { _server.ReleaseDelayedResponse(); }
    }

    [Fact]
    public async Task Disposal_Should_CancelAnActiveRequest()
    {
        // Arrange
        var transport = await TransportAsync();
        var delayed = transport.SendAsync(new HttpRequestData("GET", "delay", NoHeaders));
        try
        {
            await _server.DelayedRequestReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));

            // Act: dispose the transport while the request is active.
            await transport.DisposeAsync();

            // Assert: disposal cancels the outstanding request.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => delayed.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        // Cleanup: release the server response and dispose the transport.
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
