using System.Net;
using Core.Composition;

namespace Core.Resilience;

/// <summary>
/// The framework's named retry policies (R35, R45). Use these instead of hand-written retry loops or sleeps, so every
/// retry has bounded attempts, exponential backoff with jitter, and a log line. Never wrap assertions in a retry.
/// The implementation is the <see cref="IRetryPolicyFactory"/> selected with <c>"Resilience"</c> in
/// <c>providers.json</c> (pass <c>factory</c> to choose one explicitly, e.g. in tests).
/// </summary>
public static class RetryPolicies
{
    private static readonly HashSet<string> IdempotentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS", "PUT", "DELETE", "TRACE"
    };

    // Messages of transient network failures from .NET, Node (Playwright) and browsers.
    private static readonly string[] NetworkErrorMarkers =
    [
        "net::ERR_", "ECONNRESET", "ECONNREFUSED", "ETIMEDOUT", "EAI_AGAIN", "socket hang up"
    ];

    /// <summary>
    /// True for HTTP methods that can be safely repeated. POST and PATCH are not retried: repeating them could
    /// create or change data twice.
    /// </summary>
    public static bool IsIdempotent(string method) => IdempotentMethods.Contains(method);

    /// <summary>
    /// True when an exception looks like a transient network failure: <see cref="HttpRequestException"/> (what HTTP
    /// transports throw when no response arrived) or a message from a known network error, whatever library threw it.
    /// </summary>
    public static bool IsNetworkError(Exception ex) =>
        ex is HttpRequestException || NetworkErrorMarkers.Any(m => ex.Message.Contains(m, StringComparison.Ordinal));

    /// <summary>
    /// Retries an HTTP call whose result has status 502, 503 or 504, or that throws a network error.
    /// <paramref name="discard"/> releases a response that is about to be retried.
    /// </summary>
    public static IRetryPolicy<T> TransientHttp<T>(
        RetryOptions options, Func<T, int> statusOf, Func<T, ValueTask>? discard = null, Action<string>? log = null,
        IRetryPolicyFactory? factory = null) =>
        (factory ?? Framework.Retries).Create(
            options,
            IsNetworkError,
            r => statusOf(r) is (int)HttpStatusCode.BadGateway or (int)HttpStatusCode.ServiceUnavailable or (int)HttpStatusCode.GatewayTimeout,
            r => $"HTTP {statusOf(r)}",
            discard,
            log);

    /// <summary>Retries a page navigation that fails with a browser network error (<c>net::ERR_*</c>).</summary>
    public static IRetryPolicy Navigation(RetryOptions options, Action<string>? log = null, IRetryPolicyFactory? factory = null) =>
        Transient(options, ex => ex.Message.Contains("net::ERR_", StringComparison.Ordinal), log, factory);

    /// <summary>Retries any action whose exception <paramref name="isTransient"/> accepts. Works for sync and async code.</summary>
    public static IRetryPolicy Transient(
        RetryOptions options, Func<Exception, bool> isTransient, Action<string>? log = null, IRetryPolicyFactory? factory = null) =>
        (factory ?? Framework.Retries).Create(options, isTransient, log);
}
