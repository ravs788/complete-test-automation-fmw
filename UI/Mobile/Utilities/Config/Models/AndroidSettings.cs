using System.Collections.Generic;
namespace UI.Mobile.Utilities
{
    public partial record class ConfigSettings
    {
        public record class AndroidSettings
        {
            public string AutomationName { get; init; } = "UiAutomator2";
            public string App { get; init; } = ""; // path to .apk for Native
            public string AppPackage { get; init; } = "";
            public string AppActivity { get; init; } = "";
            public string BrowserName { get; init; } = "Chrome"; // for Web
            // Optional: supply a specific Chromedriver path or directory to avoid network downloads
            public string ChromedriverExecutable { get; init; } = "";
            public string ChromedriverExecutableDir { get; init; } = "";
            public IReadOnlyList<string> ChromedriverArgs { get; init; } = new List<string>();
            // Optional: Firefox on Android (Gecko) support
            public string GeckodriverExecutable { get; init; } = "";
            public string FirefoxPackage { get; init; } = "org.mozilla.firefox";
            public string FirefoxActivity { get; init; } = "";
        }
    }
}
