namespace UI.Mobile.Utilities
{
    public partial record class ConfigSettings
    {
        public record class BehaviorSettings
        {
            public bool NoReset { get; init; } = true;
            public bool FullReset { get; init; } = false;
            public bool AcceptAlertsAutomatically { get; init; } = true;
        }
    }
}
