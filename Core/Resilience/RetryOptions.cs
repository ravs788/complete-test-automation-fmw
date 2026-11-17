namespace Core.Resilience;

/// <summary>How often and how patiently to retry a transient failure. <c>MaxRetries = 0</c> turns retrying off.</summary>
public sealed record RetryOptions
{
    /// <summary>Extra attempts after the first one.</summary>
    public int MaxRetries { get; init; } = 2;

    /// <summary>Delay before the first retry; later retries back off exponentially with jitter.</summary>
    public int BaseDelayMs { get; init; } = 500;
}
