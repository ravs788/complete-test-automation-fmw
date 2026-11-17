namespace Core.Logging;

/// <summary>
/// Shape of <c>logging-config.json</c> (provider settings); loaded by <see cref="LoggingConfigProvider"/>. Which provider
/// is used is chosen in <c>providers.json</c> (<c>Logging</c>, <c>ResultsPublisher</c>), not here.
/// </summary>
public sealed record LoggingConfig
{
    public ElasticSection Elastic { get; init; } = new();
}
