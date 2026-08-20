namespace UI.Mobile.Utilities
{
    public partial record class ConfigSettings
    {
        public record class LoggingSettings
        {
            public string Provider { get; init; } = "console";
        }
    }
}
