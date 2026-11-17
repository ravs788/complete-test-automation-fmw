using Core.Resilience;

namespace Core.Configuration;

/// <summary>Retry settings for transient failures (R35). Assertions are never retried.</summary>
public sealed record ResilienceSettings
{
    /// <summary>Idempotent API requests (GET, PUT, DELETE, …) that fail with 502/503/504 or a network error.</summary>
    public RetryOptions Http { get; init; } = new();

    /// <summary>Page navigations that fail with a browser network error (<c>net::ERR_*</c>).</summary>
    public RetryOptions Navigation { get; init; } = new();

    /// <summary>UI actions that hit a momentary glitch (stale element, click intercepted, …).</summary>
    public RetryOptions UiAction { get; init; } = new() { BaseDelayMs = 250 };
}
