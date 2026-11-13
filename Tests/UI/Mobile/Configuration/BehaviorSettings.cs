namespace Tests.UI.Mobile.Configuration;

public sealed record BehaviorSettings
{
    public bool NoReset { get; init; } = true;
    public bool FullReset { get; init; } = false;
    public bool AcceptAlertsAutomatically { get; init; } = true;
}
