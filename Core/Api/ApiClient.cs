namespace Core.Api;

using Microsoft.Playwright;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Thin JSON client over Playwright's <see cref="IAPIRequestContext"/> (replaces RestSharp).
/// Non-2xx responses throw <see cref="ApiException"/> carrying the status code.
/// </summary>
public sealed class ApiClient : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
        var options = new APIRequestContextOptions { Method = method };
        if (body is not null)
        {
            options.DataObject = body;
        }
        if (!string.IsNullOrEmpty(token))
        {
            // Restful Booker authenticates write operations with a "token" cookie.
            options.Headers = new Dictionary<string, string> { ["Cookie"] = $"token={token}" };
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

public sealed class ApiException(string method, string endpoint, int statusCode, string statusText)
    : Exception($"{method} {endpoint} failed: {statusCode} {statusText}")
{
    public int StatusCode { get; } = statusCode;
}
