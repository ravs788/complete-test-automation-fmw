namespace Core.Logging;

public sealed record ConsoleSection
{
    public bool Enabled { get; init; } = true;
}
