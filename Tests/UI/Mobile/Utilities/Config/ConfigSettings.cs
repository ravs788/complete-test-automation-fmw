namespace UI.Mobile.Utilities
{
    // Top-level configuration model for Mobile tests.
    // Note: Nested settings classes are defined in partials:
    //  - ServerSettings.cs
    //  - DeviceSettings.cs
    //  - AndroidSettings.cs
    //  - IOSSettings.cs
    //  - BehaviorSettings.cs
    //  - LoggingSettings.cs
    //  - TestDataSettings.cs
    public partial record class ConfigSettings
    {
        // High-level
        public string Platform { get; init; } = "Android"; // Android | iOS
        public string AppType { get; init; } = "Web";      // Native | Web
        public string BaseUrl { get; init; } = "";         // Used for Mobile Web

        // Server/Appium
        public ServerSettings Server { get; init; } = new();
        public DeviceSettings Device { get; init; } = new();

        // Platform-specific blocks
        public AndroidSettings Android { get; init; } = new();
        public IOSSettings IOS { get; init; } = new();

        // Behavior toggles
        public BehaviorSettings Behavior { get; init; } = new();

        // Logging/TestData (pointers for reuse and conventions)
        public LoggingSettings Logging { get; init; } = new();
        public TestDataSettings TestData { get; init; } = new();
    }
}
