namespace Tests.UI.Mobile.Configuration;

/// <summary>
/// Settings for the Appium suite. Loaded from <c>mobilesettings.json</c> in the test output directory by
/// <see cref="MobileSettingsProvider"/>; the defaults below apply when a value is missing.
/// </summary>
public sealed record MobileSettings
{
    /// <summary>Android or iOS.</summary>
    public string Platform { get; init; } = "Android";

    /// <summary>Native or Web.</summary>
    public string AppType { get; init; } = "Web";

    /// <summary>Opened at the start of each Web test.</summary>
    public string BaseUrl { get; init; } = "";

    public ServerSettings Server { get; init; } = new();
    public DeviceSettings Device { get; init; } = new();
    public AndroidSettings Android { get; init; } = new();
    public IOSSettings IOS { get; init; } = new();
    public BehaviorSettings Behavior { get; init; } = new();
}
