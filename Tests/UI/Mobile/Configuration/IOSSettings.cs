namespace Tests.UI.Mobile.Configuration;

public sealed record IOSSettings
{
    public string AutomationName { get; init; } = "XCUITest";

    /// <summary>Path to the .app/.ipa for native tests.</summary>
    public string App { get; init; } = "";
    public string BundleId { get; init; } = "";

    /// <summary>Browser for mobile web tests.</summary>
    public string BrowserName { get; init; } = "Safari";
}
