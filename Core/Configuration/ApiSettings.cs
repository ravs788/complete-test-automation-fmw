namespace Core.Configuration;

public sealed record ApiSettings
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
