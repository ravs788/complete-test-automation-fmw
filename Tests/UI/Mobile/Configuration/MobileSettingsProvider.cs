using Core.Configuration;

namespace Tests.UI.Mobile.Configuration;

/// <summary>
/// Thread-safe, lazily loaded <see cref="MobileSettings"/>: <c>mobilesettings.json</c> (test output directory), then
/// <c>MOBILE_*</c> environment variables (e.g. <c>MOBILE_Device__Udid</c>), then the aliases <c>APPIUM_URL</c> and
/// <c>DEVICE_NAME</c>. See <see cref="SettingsLoader"/>.
/// </summary>
public static class MobileSettingsProvider
{
    public const string FileName = "mobilesettings.json";
    public const string EnvPrefix = "MOBILE_";

    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["APPIUM_URL"] = "Server:Url",
        ["DEVICE_NAME"] = "Device:DeviceName",
    };

    private static readonly Lazy<MobileSettings> _lazySettings = new(
        () => SettingsLoader.Load<MobileSettings>(AppContext.BaseDirectory, FileName, EnvPrefix, Aliases),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static MobileSettings Current => _lazySettings.Value;
}
