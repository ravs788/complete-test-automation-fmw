namespace UI.Web.Utilities
{
    // Project-specific configuration for UI Web tests
    public record class ConfigSettings
    {
        public bool RunTestsInParallel { get; init; } = true;
        public string Browser { get; init; } = "firefox";
        public bool Headless { get; init; } = false;
        public string BaseUrl { get; init; } = string.Empty;
        public string GridUrl { get; init; } = string.Empty;
        public int DriverCommandTimeoutSec { get; init; } = 60;
        public int PageLoadTimeoutSec { get; init; } = 30;
        public int ScriptTimeoutSec { get; init; } = 30;
        public int ImplicitWaitTimeoutSec { get; init; } = 0;
        public string ChromeDriverPath { get; init; } = string.Empty;
        public string FirefoxDriverPath { get; init; } = string.Empty;
        public string FirefoxBinaryPath { get; init; } = string.Empty;
        public string EdgeDriverPath { get; init; } = string.Empty;

        // Optional, for parity with API (kept to avoid breaking changes if later needed)
        public string DefaultUsername { get; init; } = string.Empty;
        public string DefaultPassword { get; init; } = string.Empty;
    }
}
