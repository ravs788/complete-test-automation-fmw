using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Composition;
using Core.Http;
using Core.Resilience;

namespace Core.Api;

/// <summary>
/// Thin JSON client over an <see cref="IHttpTransport"/> (R41), independent of any particular API or HTTP library.
/// It owns serialization, status handling (non-2xx throws <see cref="ApiException"/>) and retries; the transport
/// (selected with <c>"Http"</c> in <c>providers.json</c>) only sends. Authentication is passed in as per-request
/// <c>headers</c> built by the API-specific code (e.g. <c>RestfulBooker.AuthClient</c>).
/// </summary>
public sealed class ApiClient : IAsyncDisposable
{
    // Used for request and response bodies, so models map to the API only through [JsonPropertyName].
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly IReadOnlyDictionary<string, string> DefaultHeaders =
        new Dictionary<string, string> { ["Accept"] = "application/json" };

    private readonly IHttpTransport _transport;
    private readonly IRetryPolicy<HttpResponseData> _retry;

    private ApiClient(IHttpTransport transport, IRetryPolicy<HttpResponseData> retry)
    {
        _transport = transport;
        _retry = retry;
    }

    /// <param name="baseUrl">Base URL for relative endpoints.</param>
    /// <param name="retry">Retries for idempotent requests that hit 502/503/504 or a network error (default: 2 retries). POST and PATCH are never retried.</param>
    /// <param name="log">Optional sink for retry log lines.</param>
    /// <param name="transports">Transport to use; default: the one selected in <c>providers.json</c> (<c>"Http"</c>).</param>
    public static async Task<ApiClient> CreateAsync(
        string baseUrl, RetryOptions? retry = null, Action<string>? log = null, IHttpTransportFactory? transports = null)
    {
        transports ??= Framework.HttpTransports;
        var transport = await transports.CreateAsync(baseUrl, DefaultHeaders);
        var pipeline = RetryPolicies.TransientHttp<HttpResponseData>(retry ?? new RetryOptions(), r => r.Status, log: log);
        return new ApiClient(transport, pipeline);
    }

    public Task<T?> GetAsync<T>(string endpoint) => SendAsync<T>("GET", endpoint);

    public Task<TResponse?> PostAsync<TResponse>(string endpoint, object body, IReadOnlyDictionary<string, string>? headers = null) =>
        SendAsync<TResponse>("POST", endpoint, body, headers);

    public Task<TResponse?> PutAsync<TResponse>(string endpoint, object body, IReadOnlyDictionary<string, string>? headers = null) =>
        SendAsync<TResponse>("PUT", endpoint, body, headers);

    public Task<TResponse?> PatchAsync<TResponse>(string endpoint, object body, IReadOnlyDictionary<string, string>? headers = null) =>
        SendAsync<TResponse>("PATCH", endpoint, body, headers);

    /// <summary>Sends a DELETE and returns the status code (throws on non-2xx).</summary>
    public async Task<int> DeleteAsync(string endpoint, IReadOnlyDictionary<string, string>? headers = null)
    {
        var response = await SendRawAsync("DELETE", endpoint, headers: headers);
        EnsureSuccess("DELETE", endpoint, response);
        return response.Status;
    }

    /// <summary>Sends a request without checking the status, for tests that assert on it directly.</summary>
    public async Task<HttpResponseData> SendRawAsync(
        string method, string endpoint, object? body = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        var request = new HttpRequestData(
            method,
            endpoint,
            headers ?? new Dictionary<string, string>(),
            body is null ? null : JsonSerializer.Serialize(body, body.GetType(), JsonOptions),
            body is null ? null : "application/json");
        return RetryPolicies.IsIdempotent(method)
            ? await _retry.ExecuteAsync(ct => _transport.SendAsync(request, ct))
            : await _transport.SendAsync(request);
    }

    private async Task<T?> SendAsync<T>(
        string method, string endpoint, object? body = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        var response = await SendRawAsync(method, endpoint, body, headers);
        EnsureSuccess(method, endpoint, response);
        return JsonSerializer.Deserialize<T>(response.Body, JsonOptions);
    }

    private static void EnsureSuccess(string method, string endpoint, HttpResponseData response)
    {
        if (!response.Ok)
        {
            throw new ApiException(method, endpoint, response.Status, response.StatusText);
        }
    }

    public ValueTask DisposeAsync() => _transport.DisposeAsync();
}
