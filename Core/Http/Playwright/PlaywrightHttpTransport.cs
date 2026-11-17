using Microsoft.Playwright;

namespace Core.Http.Playwright;

/// <summary><see cref="IHttpTransport"/> on a Playwright <see cref="IAPIRequestContext"/> (one per transport).</summary>
public sealed class PlaywrightHttpTransport(IAPIRequestContext context) : IHttpTransport
{
    public async Task<HttpResponseData> SendAsync(HttpRequestData request, CancellationToken cancellationToken = default)
    {
        var headers = new Dictionary<string, string>(request.Headers);
        var options = new APIRequestContextOptions { Method = request.Method, Headers = headers };
        if (request.Body is not null)
        {
            options.DataString = request.Body;
            headers["Content-Type"] = request.ContentType ?? "text/plain";
        }

        IAPIResponse response;
        try
        {
            response = await context.FetchAsync(request.Url, options);
        }
        catch (PlaywrightException ex)
        {
            // No response arrived: report it the framework way, so retries don't depend on Playwright (R41).
            throw new HttpRequestException(ex.Message, ex);
        }

        try
        {
            var body = await response.BodyAsync();
            var responseHeaders = response.Headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
            return new HttpResponseData(response.Status, response.StatusText, responseHeaders, body);
        }
        finally
        {
            await response.DisposeAsync();
        }
    }

    public ValueTask DisposeAsync() => context.DisposeAsync();
}
