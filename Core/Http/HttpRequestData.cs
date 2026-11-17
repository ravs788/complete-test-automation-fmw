namespace Core.Http;

/// <summary>
/// One HTTP request as the framework describes it (R41): independent of the HTTP library that sends it.
/// </summary>
/// <param name="Method">HTTP method, e.g. <c>GET</c>.</param>
/// <param name="Url">Relative to the transport's base URL, or absolute.</param>
/// <param name="Headers">Request headers, e.g. <c>Cookie</c> for auth.</param>
/// <param name="Body">Body text, or null for none.</param>
/// <param name="ContentType">Content type of <paramref name="Body"/>, e.g. <c>application/json</c>.</param>
public sealed record HttpRequestData(
    string Method,
    string Url,
    IReadOnlyDictionary<string, string> Headers,
    string? Body = null,
    string? ContentType = null);
