using System.Net.Http.Headers;
using System.Text;

namespace Core.Http;

/// <summary>Built-in <see cref="IHttpTransport"/> on .NET's <see cref="HttpClient"/> (R41): needs no browser library.</summary>
public sealed class HttpClientTransport : IHttpTransport
{
    private readonly HttpClient _client;

    public HttpClientTransport(string baseUrl, IReadOnlyDictionary<string, string> defaultHeaders)
    {
        // Cookies are sent as request headers (e.g. Restful Booker's token), so the handler must not manage its own.
        _client = new HttpClient(new HttpClientHandler { UseCookies = false })
        {
            BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/"),
        };
        foreach (var (name, value) in defaultHeaders)
        {
            _client.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        }
    }

    public async Task<HttpResponseData> SendAsync(HttpRequestData request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(new HttpMethod(request.Method), request.Url);
        if (request.Body is not null)
        {
            message.Content = new StringContent(request.Body, Encoding.UTF8);
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType ?? "text/plain");
        }
        foreach (var (name, value) in request.Headers)
        {
            if (!message.Headers.TryAddWithoutValidation(name, value))
            {
                message.Content?.Headers.TryAddWithoutValidation(name, value);
            }
        }

        using var response = await _client.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var headers = response.Headers.Concat(response.Content.Headers)
            .GroupBy(h => h.Key.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => string.Join(", ", g.SelectMany(h => h.Value)));
        return new HttpResponseData((int)response.StatusCode, response.ReasonPhrase ?? "", headers, body);
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
