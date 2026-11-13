namespace Core.Logging;

public sealed record ElasticSection
{
    public string? Url { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
}
