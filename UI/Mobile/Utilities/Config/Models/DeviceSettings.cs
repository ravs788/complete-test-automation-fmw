namespace UI.Mobile.Utilities
{
    public partial record class ConfigSettings
    {
        public record class DeviceSettings
        {
            public string DeviceName { get; init; } = "Android Emulator";
            public string PlatformVersion { get; init; } = "";
            public string Udid { get; init; } = "";
            public string Orientation { get; init; } = "portrait"; // portrait | landscape
        }
    }
}
