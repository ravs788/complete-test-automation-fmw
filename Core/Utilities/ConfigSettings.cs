namespace Core.Utilities
{
    public class ConfigSettings
    {
        public bool RunTestsInParallel { get; set; } = true;
        public string Browser { get; set; } = "firefox";
        public bool Headless { get; set; } = false;
        public string BaseUrl { get; set; } = "https://www.saucedemo.com/";
    }
}
