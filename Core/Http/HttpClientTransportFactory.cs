namespace Core.Http;

/// <summary>Creates <see cref="HttpClientTransport"/>s (the built-in <c>httpclient</c> provider).</summary>
public sealed class HttpClientTransportFactory : IHttpTransportFactory
{
    public Task<IHttpTransport> CreateAsync(string baseUrl, IReadOnlyDictionary<string, string> defaultHeaders) =>
        Task.FromResult<IHttpTransport>(new HttpClientTransport(baseUrl, defaultHeaders));
}
