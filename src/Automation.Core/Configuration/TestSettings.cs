namespace Automation.Core.Configuration;

public sealed record TestSettings
{
    public string BaseUrl { get; init; } = "https://demo.playwright.dev/todomvc";
    public string Browser { get; init; } = "chromium";
    public bool Headless { get; init; } = true;
    public float DefaultTimeoutMs { get; init; } = 30_000f;
}
