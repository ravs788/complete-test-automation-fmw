using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Playwright;

namespace Core.Api;

/// <summary>
/// Thin JSON client over Playwright's <see cref="IAPIRequestContext"/>.
/// Non-2xx responses throw <see cref="ApiException"/> carrying the status code.
/// </summary>
public sealed class ApiClient : IAsyncDisposable
{
    // Used for request and response bodies, so models map to the API only through [JsonPropertyName].
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IAPIRequestContext _request;

    private ApiClient(IAPIRequestContext request) => _request = request;

    public static async Task<ApiClient> CreateAsync(IPlaywright playwright, string baseUrl)
    {
        var request = await playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = baseUrl,
            ExtraHTTPHeaders = new Dictionary<string, string> { ["Accept"] = "application/json" }
        });
        return new ApiClient(request);
    }

    public Task<T?> GetAsync<T>(string endpoint) => SendAsync<T>("GET", endpoint);

    public Task<TResponse?> PostAsync<TResponse>(string endpoint, object body, string? token = null) =>
        SendAsync<TResponse>("POST", endpoint, body, token);

    public Task<TResponse?> PutAsync<TResponse>(string endpoint, object body, string? token = null) =>
        SendAsync<TResponse>("PUT", endpoint, body, token);

    public Task<TResponse?> PatchAsync<TResponse>(string endpoint, object body, string? token = null) =>
        SendAsync<TResponse>("PATCH", endpoint, body, token);

    /// <summary>Sends a DELETE and returns the status code (throws on non-2xx).</summary>
    public async Task<int> DeleteAsync(string endpoint, string? token = null)
    {
        var response = await SendRawAsync("DELETE", endpoint, token: token);
        EnsureSuccess("DELETE", endpoint, response);
        return response.Status;
    }

    /// <summary>Sends a request without checking the status, for tests that assert on it directly.</summary>
    public async Task<IAPIResponse> SendRawAsync(string method, string endpoint, object? body = null, string? token = null)
    {
        var headers = new Dictionary<string, string>();
        var options = new APIRequestContextOptions { Method = method, Headers = headers };
        if (body is not null)
        {
            options.DataString = JsonSerializer.Serialize(body, body.GetType(), JsonOptions);
            headers["Content-Type"] = "application/json";
        }
        if (!string.IsNullOrEmpty(token))
        {
            // Restful Booker authenticates write operations with a "token" cookie.
            headers["Cookie"] = $"token={token}";
        }
        return await _request.FetchAsync(endpoint, options);
    }

    private async Task<T?> SendAsync<T>(string method, string endpoint, object? body = null, string? token = null)
    {
        var response = await SendRawAsync(method, endpoint, body, token);
        EnsureSuccess(method, endpoint, response);
        return JsonSerializer.Deserialize<T>(await response.BodyAsync(), JsonOptions);
    }

    private static void EnsureSuccess(string method, string endpoint, IAPIResponse response)
    {
        if (!response.Ok)
        {
            throw new ApiException(method, endpoint, response.Status, response.StatusText);
        }
    }

    public async ValueTask DisposeAsync() => await _request.DisposeAsync();
}
