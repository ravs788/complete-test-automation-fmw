namespace Core.Resilience;

/// <summary>
/// Runs an action, retrying it when it fails with an exception the policy treats as transient (R45). Created by
/// <see cref="RetryPolicies"/> through the <see cref="IRetryPolicyFactory"/> selected in <c>providers.json</c>. When
/// retries are exhausted the last exception is rethrown unchanged.
/// </summary>
public interface IRetryPolicy
{
    void Execute(Action action);

    T Execute<T>(Func<T> action);

    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}

/// <summary>
/// Like <see cref="IRetryPolicy"/>, and also retries when the <em>result</em> is transient (e.g. HTTP 503). When
/// retries are exhausted the last result is returned.
/// </summary>
public interface IRetryPolicy<T>
{
    Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}
