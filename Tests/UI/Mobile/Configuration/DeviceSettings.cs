namespace Tests.UI.Mobile.Configuration;

public sealed record DeviceSettings
{
    public string DeviceName { get; init; } = "Android Emulator";
    public string PlatformVersion { get; init; } = "";
    public string Udid { get; init; } = "";

    /// <summary>portrait or landscape.</summary>
    public string Orientation { get; init; } = "portrait";
}
