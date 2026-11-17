namespace Core.Http;

/// <summary>
/// Creates <see cref="IHttpTransport"/>s (R41). Registered by name through an <c>IFrameworkPlugin</c> and selected with
/// <c>"Http"</c> in <c>providers.json</c>; <c>httpclient</c> is built in.
/// </summary>
public interface IHttpTransportFactory
{
    /// <summary>A transport for <paramref name="baseUrl"/> that sends <paramref name="defaultHeaders"/> with every request.</summary>
    Task<IHttpTransport> CreateAsync(string baseUrl, IReadOnlyDictionary<string, string> defaultHeaders);
}
