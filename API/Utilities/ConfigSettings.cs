namespace API.Utilities
{
    // Project-specific configuration for API tests
    public record class ConfigSettings
    {
        public string BaseUrl { get; init; } = string.Empty;
        public string DefaultUsername { get; init; } = string.Empty;
        public string DefaultPassword { get; init; } = string.Empty;
    }
}
