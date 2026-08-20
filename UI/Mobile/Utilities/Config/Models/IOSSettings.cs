namespace UI.Mobile.Utilities
{
    public partial record class ConfigSettings
    {
        public record class IOSSettings
        {
            public string AutomationName { get; init; } = "XCUITest";
            public string App { get; init; } = ""; // path to .app/.ipa for Native
            public string BundleId { get; init; } = "";
            public string BrowserName { get; init; } = "Safari"; // for Web
        }
    }
}
