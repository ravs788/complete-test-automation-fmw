namespace Core.Ui;

/// <summary>A launched browser (R42), usually one per test class; opens a fresh <see cref="IUiSession"/> per test.</summary>
public interface IUiDriver : IAsyncDisposable
{
    Task<IUiSession> NewSessionAsync(UiSessionOptions options);
}

/// <summary>Launches browsers (R42). Registered by name through an <c>IFrameworkPlugin</c>; selected with <c>"Ui"</c> in <c>providers.json</c>.</summary>
public interface IUiDriverFactory
{
    Task<IUiDriver> LaunchAsync(UiLaunchOptions options);
}

/// <param name="Browser">Driver-specific browser name, e.g. <c>chromium</c>, <c>chrome</c>, <c>firefox</c>.</param>
/// <param name="Headless">Run without a visible window.</param>
public sealed record UiLaunchOptions(string Browser, bool Headless);

/// <param name="BaseUrl">Base for relative URLs.</param>
/// <param name="DefaultTimeoutMs">Timeout for actions (assertions use the driver's own assertion timeout).</param>
public sealed record UiSessionOptions(string BaseUrl, float DefaultTimeoutMs);
