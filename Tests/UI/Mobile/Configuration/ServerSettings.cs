namespace Tests.UI.Mobile.Configuration;

public sealed record ServerSettings
{
    public string Url { get; init; } = "http://127.0.0.1:4723/";
    public int CommandTimeoutSec { get; init; } = 120;
    public int NewCommandTimeoutSec { get; init; } = 120;
}
