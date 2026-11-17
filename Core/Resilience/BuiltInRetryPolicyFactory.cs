namespace Core.Resilience;

/// <summary>
/// Built-in <see cref="IRetryPolicyFactory"/> (<c>builtin</c>, R45): a plain loop with exponential backoff and jitter,
/// no library needed.
/// </summary>
public sealed class BuiltInRetryPolicyFactory : IRetryPolicyFactory
{
    public IRetryPolicy Create(RetryOptions options, Func<Exception, bool> isTransient, Action<string>? log = null) =>
        new Policy(options, isTransient, log);

    public IRetryPolicy<T> Create<T>(
        RetryOptions options, Func<Exception, bool> isTransient, Func<T, bool> isTransientResult, Func<T, string> describe,
        Func<T, ValueTask>? discard = null, Action<string>? log = null) =>
        new ResultPolicy<T>(options, isTransient, isTransientResult, describe, discard, log);

    /// <summary>Delay before retry number <paramref name="retry"/> (0-based): base × 2^retry, ±25 % jitter.</summary>
    internal static TimeSpan DelayFor(RetryOptions options, int retry)
    {
        var baseMs = Math.Max(0, options.BaseDelayMs) * Math.Pow(2, retry);
        return TimeSpan.FromMilliseconds(baseMs * (0.75 + (Random.Shared.NextDouble() * 0.5)));
    }

    private sealed class Policy(RetryOptions options, Func<Exception, bool> isTransient, Action<string>? log) : IRetryPolicy
    {
        public void Execute(Action action) => Execute<object?>(() => { action(); return null; });

        public T Execute<T>(Func<T> action)
        {
            for (var retry = 0; ; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex) when (retry < options.MaxRetries && isTransient(ex))
                {
                    var delay = DelayFor(options, retry);
                    log?.Invoke(RetryLog.Line(retry + 1, options.MaxRetries, delay, ex.Message));
                    Thread.Sleep(delay);
                }
            }
        }

        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
            ExecuteAsync<object?>(async ct => { await action(ct); return null; }, cancellationToken);

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            for (var retry = 0; ; retry++)
            {
                try
                {
                    return await action(cancellationToken);
                }
                catch (Exception ex) when (retry < options.MaxRetries && isTransient(ex))
                {
                    var delay = DelayFor(options, retry);
                    log?.Invoke(RetryLog.Line(retry + 1, options.MaxRetries, delay, ex.Message));
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }

    private sealed class ResultPolicy<T>(
        RetryOptions options, Func<Exception, bool> isTransient, Func<T, bool> isTransientResult, Func<T, string> describe,
        Func<T, ValueTask>? discard, Action<string>? log) : IRetryPolicy<T>
    {
        public async Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            for (var retry = 0; ; retry++)
            {
                T result;
                try
                {
                    result = await action(cancellationToken);
                }
                catch (Exception ex) when (retry < options.MaxRetries && isTransient(ex))
                {
                    var delay = DelayFor(options, retry);
                    log?.Invoke(RetryLog.Line(retry + 1, options.MaxRetries, delay, ex.Message));
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                if (retry >= options.MaxRetries || !isTransientResult(result))
                {
                    return result;
                }
                var resultDelay = DelayFor(options, retry);
                log?.Invoke(RetryLog.Line(retry + 1, options.MaxRetries, resultDelay, describe(result)));
                if (discard is not null)
                {
                    await discard(result);
                }
                await Task.Delay(resultDelay, cancellationToken);
            }
        }
    }
}
