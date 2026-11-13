namespace Core.Api;

/// <summary>Thrown by <see cref="ApiClient"/> when a response is not 2xx.</summary>
public sealed class ApiException(string method, string endpoint, int statusCode, string statusText)
    : Exception($"{method} {endpoint} failed: {statusCode} {statusText}")
{
    public int StatusCode { get; } = statusCode;
}
