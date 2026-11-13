namespace Core.Logging;

/// <summary>Shape of <c>logging-config.json</c>; loaded by <see cref="LoggingConfigProvider"/>.</summary>
public sealed record LoggingConfig
{
    /// <summary>Selected provider: "elastic" or "console".</summary>
    public string Provider { get; init; } = "elastic";

    public ElasticSection Elastic { get; init; } = new();
}
