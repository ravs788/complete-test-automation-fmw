namespace Tests.UI.Mobile.Configuration;

public sealed record AndroidSettings
{
    public string AutomationName { get; init; } = "UiAutomator2";

    // Native apps
    public string App { get; init; } = "";
    public string AppPackage { get; init; } = "";
    public string AppActivity { get; init; } = "";

    // Mobile web
    public string BrowserName { get; init; } = "Chrome";

    /// <summary>Optional Chromedriver path or directory; when both are empty Appium downloads a matching one.</summary>
    public string ChromedriverExecutable { get; init; } = "";
    public string ChromedriverExecutableDir { get; init; } = "";
    public IReadOnlyList<string> ChromedriverArgs { get; init; } = [];

    // Firefox on Android (GeckoDriver)
    public string GeckodriverExecutable { get; init; } = "";
    public string FirefoxPackage { get; init; } = "org.mozilla.firefox";
    public string FirefoxActivity { get; init; } = "";
}
