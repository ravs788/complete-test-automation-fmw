namespace Core.Resilience;

/// <summary>
/// Builds retry policies (R45). Registered by name through an <c>IFrameworkPlugin</c> and selected with
/// <c>"Resilience"</c> in <c>providers.json</c>; <c>builtin</c> ships with Core, <c>polly</c> is an adapter.
/// Every implementation: exponential backoff from <see cref="RetryOptions.BaseDelayMs"/> with jitter, at most
/// <see cref="RetryOptions.MaxRetries"/> retries (none when it is 0 or less), and one log line per retry:
/// <c>[Retry] attempt N of M after D ms: reason</c>.
/// </summary>
public interface IRetryPolicyFactory
{
    IRetryPolicy Create(RetryOptions options, Func<Exception, bool> isTransient, Action<string>? log = null);

    /// <param name="options">Retry count and base delay.</param>
    /// <param name="isTransient">True for an exception worth retrying.</param>
    /// <param name="isTransientResult">True for a result worth retrying.</param>
    /// <param name="describe">Text for the log line when a result is retried.</param>
    /// <param name="discard">Releases a result that is about to be retried.</param>
    /// <param name="log">Receives one line per retry.</param>
    IRetryPolicy<T> Create<T>(
        RetryOptions options, Func<Exception, bool> isTransient, Func<T, bool> isTransientResult, Func<T, string> describe,
        Func<T, ValueTask>? discard = null, Action<string>? log = null);
}
