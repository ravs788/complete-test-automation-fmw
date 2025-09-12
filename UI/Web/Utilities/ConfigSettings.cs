namespace UI.Web.Utilities
{
    // Project-specific configuration for UI Web tests
    public class ConfigSettings
    {
        public bool RunTestsInParallel { get; set; } = true;
        public string Browser { get; set; } = "firefox";
        public bool Headless { get; set; } = false;
        public string BaseUrl { get; set; } = string.Empty;

        // Optional, for parity with API (kept to avoid breaking changes if later needed)
        public string DefaultUsername { get; set; } = string.Empty;
        public string DefaultPassword { get; set; } = string.Empty;
    }
}
