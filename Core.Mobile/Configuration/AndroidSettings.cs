namespace Core.Mobile.Configuration;

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

    // UiAutomator2 timeouts (milliseconds). The defaults are Appium's own; slow emulators (e.g. the hosted CI
    // agent, where Android gets a single vCPU) need more: the CI script raises them through MOBILE_Android__*.

    /// <summary>Limit for each adb command Appium runs (<c>appium:adbExecTimeout</c>).</summary>
    public int AdbExecTimeoutMs { get; init; } = 20_000;

    /// <summary>Limit for installing Appium's UiAutomator2 server apps (<c>appium:uiautomator2ServerInstallTimeout</c>).</summary>
    public int ServerInstallTimeoutMs { get; init; } = 20_000;

    /// <summary>Limit for the UiAutomator2 server to start (<c>appium:uiautomator2ServerLaunchTimeout</c>).</summary>
    public int ServerLaunchTimeoutMs { get; init; } = 30_000;
}
