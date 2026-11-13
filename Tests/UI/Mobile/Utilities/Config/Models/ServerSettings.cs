namespace UI.Mobile.Utilities
{
    public partial record class ConfigSettings
    {
        // Nested settings
        public record class ServerSettings
        {
            public string Url { get; init; } = "http://127.0.0.1:4723/";
            public int CommandTimeoutSec { get; init; } = 120;
            public int NewCommandTimeoutSec { get; init; } = 120;
        }
    }
}
