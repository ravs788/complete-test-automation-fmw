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

    /// <summary>
    /// Playwright trace recording: <c>off</c>, <c>on-failure</c> (attach the trace to Allure when a test fails) or
    /// <c>on</c> (attach it for every test). Env override: TRACE.
    /// </summary>
    public string Trace { get; init; } = TraceModes.OnFailure;

    public WebAppSettings TodoMvc { get; init; } = new() { BaseUrl = "https://demo.playwright.dev/todomvc/" };

    public WebAppSettings SauceDemo { get; init; } = new() { BaseUrl = "https://www.saucedemo.com/" };

    public ApiSettings RestfulBooker { get; init; } = new() { BaseUrl = "https://restful-booker.herokuapp.com/" };
}
