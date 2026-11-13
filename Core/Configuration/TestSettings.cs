namespace Core.Configuration;

/// <summary>
/// Settings for the Playwright suites. Loaded from <c>testsettings.json</c> in the test output
/// directory by <see cref="ConfigurationProvider"/>; the defaults below apply when a value is missing.
/// </summary>
public sealed record TestSettings
{
    /// <summary>chromium (bundled), chrome, msedge (installed browsers), firefox or webkit. Env override: BROWSER.</summary>
    public string Browser { get; init; } = "chromium";

    /// <summary>Env override: HEADLESS=false to watch the browser.</summary>
    public bool Headless { get; init; } = true;

    public float DefaultTimeoutMs { get; init; } = 30_000f;

    public WebAppSettings TodoMvc { get; init; } = new() { BaseUrl = "https://demo.playwright.dev/todomvc" };

    public WebAppSettings SauceDemo { get; init; } = new() { BaseUrl = "https://www.saucedemo.com/" };

    public ApiSettings RestfulBooker { get; init; } = new() { BaseUrl = "https://restful-booker.herokuapp.com/" };
}

public sealed record WebAppSettings
{
    public string BaseUrl { get; init; } = string.Empty;
}

public sealed record ApiSettings
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
