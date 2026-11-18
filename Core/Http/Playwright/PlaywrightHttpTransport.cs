using Microsoft.Playwright;

namespace Core.Http.Playwright;

/// <summary>
/// HTTP on Playwright. Each request owns its context (no implicit cookie jar, like HttpClientTransport), so
/// cancellation can abort the native request without affecting concurrent requests or future use of the transport.
/// </summary>
public sealed class PlaywrightHttpTransport(IPlaywright playwright, string baseUrl, IReadOnlyDictionary<string, string> defaultHeaders) : IHttpTransport
{
    private readonly IReadOnlyDictionary<string, string> _defaultHeaders = new Dictionary<string, string>(defaultHeaders);
    private readonly object _gate = new();
    private readonly HashSet<IAPIRequestContext> _contexts = [];
    private readonly CancellationTokenSource _stop = new();
    private bool _disposed;

    public async Task<HttpResponseData> SendAsync(HttpRequestData request, CancellationToken cancellationToken = default)
    {
        CancellationTokenSource linked;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stop.Token);
        }
        using var cancellation = linked;
        var token = linked.Token;
        token.ThrowIfCancellationRequested();
        await using var context = await playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = baseUrl,
            ExtraHTTPHeaders = _defaultHeaders
        });
        lock (_gate)
        {
            token.ThrowIfCancellationRequested();
            _contexts.Add(context);
        }
        try
        {
            var operation = SendCoreAsync(context, request);
            try
            {
                return await operation.WaitAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Abort the actual network operation and observe its completion, not just the caller's wait.
                await context.DisposeAsync();
                try { await operation; } catch { /* cancellation is the caller-visible outcome */ }
                throw;
            }
            catch (PlaywrightException ex) when (token.IsCancellationRequested)
            {
                throw new OperationCanceledException("HTTP request cancelled.", ex, token);
            }
            catch (PlaywrightException ex)
            {
                throw new HttpRequestException(ex.Message, ex);
            }
        }
        finally
        {
            lock (_gate) { _contexts.Remove(context); }
        }
    }

    private static async Task<HttpResponseData> SendCoreAsync(IAPIRequestContext context, HttpRequestData request)
    {
        var headers = new Dictionary<string, string>(request.Headers);
        var options = new APIRequestContextOptions { Method = request.Method, Headers = headers };
        if (request.Body is not null)
        {
            options.DataString = request.Body;
            headers["Content-Type"] = request.ContentType ?? "text/plain";
        }
        var response = await context.FetchAsync(request.Url, options);
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

    public async ValueTask DisposeAsync()
    {
        IAPIRequestContext[] contexts;
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _stop.Cancel();
            contexts = _contexts.ToArray();
        }
        try
        {
            await Task.WhenAll(contexts.Select(c => c.DisposeAsync().AsTask()));
        }
        finally
        {
            _stop.Dispose();
        }
    }
}
