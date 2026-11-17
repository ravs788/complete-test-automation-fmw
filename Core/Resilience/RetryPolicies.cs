using Microsoft.Playwright;
using Polly;
using Polly.Retry;

namespace Core.Resilience;

/// <summary>
/// The framework's retry policies (Polly). Use these instead of hand-written retry loops or sleeps, so every retry has
/// bounded attempts, exponential backoff with jitter, and a log line. Never wrap assertions in a retry.
/// </summary>
public static class RetryPolicies
{
    private static readonly HashSet<string> IdempotentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS", "PUT", "DELETE", "TRACE"
    };

    private static readonly string[] NetworkErrorMarkers =
    [
        "net::ERR_", "ECONNRESET", "ECONNREFUSED", "ETIMEDOUT", "EAI_AGAIN", "socket hang up"
    ];

    /// <summary>
    /// True for HTTP methods that can be safely repeated. POST and PATCH are not retried: repeating them could
    /// create or change data twice.
    /// </summary>
    public static bool IsIdempotent(string method) => IdempotentMethods.Contains(method);

    /// <summary>True when an exception looks like a transient network failure (browser or Node networking).</summary>
    public static bool IsNetworkError(Exception ex) =>
        ex is HttpRequestException ||
        (ex is PlaywrightException && NetworkErrorMarkers.Any(m => ex.Message.Contains(m, StringComparison.Ordinal)));

    /// <summary>
    /// Retries an HTTP call whose result has status 502, 503 or 504, or that throws a network error.
    /// <paramref name="discard"/> releases a response that is about to be retried.
    /// </summary>
    public static ResiliencePipeline<T> TransientHttp<T>(
        RetryOptions options, Func<T, int> statusOf, Func<T, ValueTask>? discard = null, Action<string>? log = null)
    {
        if (options.MaxRetries <= 0)
        {
            return ResiliencePipeline<T>.Empty;
        }

        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = options.MaxRetries,
                Delay = TimeSpan.FromMilliseconds(options.BaseDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .HandleResult(r => statusOf(r) is 502 or 503 or 504)
                    .Handle<Exception>(IsNetworkError),
                OnRetry = async args =>
                {
                    var reason = args.Outcome.Exception?.Message ?? $"HTTP {statusOf(args.Outcome.Result!)}";
                    log?.Invoke($"[Retry] attempt {args.AttemptNumber + 1} of {options.MaxRetries} after {args.RetryDelay.TotalMilliseconds:F0} ms: {FirstLine(reason)}");
                    if (args.Outcome.Result is { } result && discard is not null)
                    {
                        await discard(result);
                    }
                }
            })
            .Build();
    }

    /// <summary>Retries a page navigation that fails with a browser network error (<c>net::ERR_*</c>).</summary>
    public static ResiliencePipeline Navigation(RetryOptions options, Action<string>? log = null) =>
        Transient(options, ex => ex is PlaywrightException && ex.Message.Contains("net::ERR_", StringComparison.Ordinal), log);

    /// <summary>Retries any action whose exception <paramref name="isTransient"/> accepts. Works for sync and async code.</summary>
    public static ResiliencePipeline Transient(RetryOptions options, Func<Exception, bool> isTransient, Action<string>? log = null)
    {
        if (options.MaxRetries <= 0)
        {
            return ResiliencePipeline.Empty;
        }

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetries,
                Delay = TimeSpan.FromMilliseconds(options.BaseDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle(isTransient),
                OnRetry = args =>
                {
                    log?.Invoke($"[Retry] attempt {args.AttemptNumber + 1} of {options.MaxRetries} after {args.RetryDelay.TotalMilliseconds:F0} ms: {FirstLine(args.Outcome.Exception?.Message)}");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    private static string FirstLine(string? text) => (text ?? "").Split('\n')[0].Trim();
}
