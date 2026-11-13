namespace UI.Mobile.Utilities
{
    public partial record class ConfigSettings
    {
        public record class TestDataSettings
        {
            public string BasePath { get; init; } = "UI/Mobile/test-data";
        }
    }
}
