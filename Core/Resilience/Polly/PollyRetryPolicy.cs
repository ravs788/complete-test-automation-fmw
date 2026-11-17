using Polly;

namespace Core.Resilience.Polly;

/// <summary><see cref="IRetryPolicy"/> over a Polly <see cref="ResiliencePipeline"/>.</summary>
public sealed class PollyRetryPolicy(ResiliencePipeline pipeline) : IRetryPolicy
{
    public void Execute(Action action) => pipeline.Execute(action);

    public T Execute<T>(Func<T> action) => pipeline.Execute(_ => action());

    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken).AsTask();

    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken).AsTask();
}

/// <summary><see cref="IRetryPolicy{T}"/> over a Polly <see cref="ResiliencePipeline{T}"/>.</summary>
public sealed class PollyRetryPolicy<T>(ResiliencePipeline<T> pipeline) : IRetryPolicy<T>
{
    public Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
        pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken).AsTask();
}
