namespace Core.Configuration;

public sealed record WebAppSettings
{
    public string BaseUrl { get; init; } = string.Empty;
}
