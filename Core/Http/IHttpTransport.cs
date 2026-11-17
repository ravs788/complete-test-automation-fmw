namespace Core.Http;

/// <summary>
/// Sends HTTP requests for <see cref="Core.Api.ApiClient"/> (R41). Implementations do <b>not</b> retry, serialize or
/// check status codes (that is <c>ApiClient</c>'s job): they return any HTTP response as is and throw
/// <see cref="HttpRequestException"/> only when no response arrived (network failure), so retries work the same for
/// every transport.
/// </summary>
public interface IHttpTransport : IAsyncDisposable
{
    Task<HttpResponseData> SendAsync(HttpRequestData request, CancellationToken cancellationToken = default);
}
