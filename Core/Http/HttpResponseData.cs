using System.Text;

namespace Core.Http;

/// <summary>
/// One HTTP response as the framework sees it (R41): fully read, so it needs no disposing and doesn't depend on the
/// HTTP library that received it. <see cref="Core.Api.ApiClient.SendRawAsync"/> returns it for status assertions.
/// </summary>
public sealed record HttpResponseData(int Status, string StatusText, IReadOnlyDictionary<string, string> Headers, byte[] Body)
{
    /// <summary>True for 2xx.</summary>
    public bool Ok => Status is >= 200 and <= 299;

    /// <summary>The body as UTF-8 text.</summary>
    public string Text => Encoding.UTF8.GetString(Body);
}
