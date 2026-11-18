using System.Text;
using Core.Api;
using Core.Http;
using Core.Resilience;

namespace Tests.Unit.Api;

/// <summary>ApiClient's own behaviour on top of any transport (R41): JSON, status handling and retries.</summary>
public class ApiClientTests
{
    private sealed class ScriptedTransport(params Func<HttpRequestData, HttpResponseData>[] responses) : IHttpTransport, IHttpTransportFactory
    {
        public List<HttpRequestData> Requests { get; } = [];

        public Task<HttpResponseData> SendAsync(HttpRequestData request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(responses[Math.Min(Requests.Count, responses.Length) - 1](request));
        }

        public Task<IHttpTransport> CreateAsync(string baseUrl, IReadOnlyDictionary<string, string> defaultHeaders) =>
            Task.FromResult<IHttpTransport>(this);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed record Item(string Name);

    private static readonly RetryOptions FastRetry = new() { MaxRetries = 2, BaseDelayMs = 1 };

    private static HttpResponseData Status(int status, string body = "") =>
        new(status, status == 200 ? "OK" : "Error", new Dictionary<string, string>(), Encoding.UTF8.GetBytes(body));

    [Fact]
    public async Task Get_Should_RetryTransientStatus_ThenDeserialize()
    {
        // Arrange
        var transport = new ScriptedTransport(_ => Status(503), _ => Status(200, "{\"name\":\"ok\"}"));
        await using var client = await ApiClient.CreateAsync("http://unused/", FastRetry, transports: transport);

        // Act
        var item = await client.GetAsync<Item>("items/1");

        // Assert
        Assert.Equal("ok", item!.Name);
        Assert.Equal(2, transport.Requests.Count);
    }

    [Fact]
    public async Task Post_Should_NeverBeRetried_And_SendJson()
    {
        // Arrange
        var transport = new ScriptedTransport(_ => Status(503));
        await using var client = await ApiClient.CreateAsync("http://unused/", FastRetry, transports: transport);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => client.PostAsync<Item>("items", new Item("new")));

        // Assert: POST was sent once with the expected JSON and content type.
        Assert.Equal(503, ex.StatusCode);
        var request = Assert.Single(transport.Requests);
        Assert.Equal("{\"Name\":\"new\"}", request.Body);
        Assert.Equal("application/json", request.ContentType);
    }

    [Fact]
    public async Task Get_Should_RetryNetworkFailures()
    {
        // Arrange
        var calls = 0;
        var transport = new ScriptedTransport(_ => ++calls == 1 ? throw new HttpRequestException("connection reset") : Status(200, "{\"name\":\"x\"}"));
        await using var client = await ApiClient.CreateAsync("http://unused/", FastRetry, transports: transport);

        // Act & Assert
        Assert.Equal("x", (await client.GetAsync<Item>("items/2"))!.Name);
    }

    [Fact]
    public async Task SendRaw_Should_ReturnNonSuccess_WithoutThrowing()
    {
        // Arrange
        var transport = new ScriptedTransport(_ => Status(404, "missing"));
        await using var client = await ApiClient.CreateAsync("http://unused/", FastRetry, transports: transport);

        // Act
        var response = await client.SendRawAsync("GET", "items/3");

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Equal("missing", response.Text);
    }
}
