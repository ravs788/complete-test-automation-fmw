using Polly;
using Polly.Retry;

namespace Core.Resilience.Polly;

/// <summary><see cref="IRetryPolicyFactory"/> on Polly (<c>polly</c>): exponential backoff with Polly's jitter.</summary>
public sealed class PollyRetryPolicyFactory : IRetryPolicyFactory
{
    public IRetryPolicy Create(RetryOptions options, Func<Exception, bool> isTransient, Action<string>? log = null)
    {
        if (options.MaxRetries <= 0)
        {
            return new PollyRetryPolicy(ResiliencePipeline.Empty);
        }

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetries,
                Delay = TimeSpan.FromMilliseconds(options.BaseDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle(isTransient),
                OnRetry = args =>
                {
                    log?.Invoke(RetryLog.Line(args.AttemptNumber + 1, options.MaxRetries, args.RetryDelay, args.Outcome.Exception?.Message));
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
        return new PollyRetryPolicy(pipeline);
    }

    public IRetryPolicy<T> Create<T>(
        RetryOptions options, Func<Exception, bool> isTransient, Func<T, bool> isTransientResult, Func<T, string> describe,
        Func<T, ValueTask>? discard = null, Action<string>? log = null)
    {
        if (options.MaxRetries <= 0)
        {
            return new PollyRetryPolicy<T>(ResiliencePipeline<T>.Empty);
        }

        var pipeline = new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = options.MaxRetries,
                Delay = TimeSpan.FromMilliseconds(options.BaseDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .HandleResult(isTransientResult)
                    .Handle<Exception>(isTransient),
                OnRetry = async args =>
                {
                    var reason = args.Outcome.Exception?.Message ?? describe(args.Outcome.Result!);
                    log?.Invoke(RetryLog.Line(args.AttemptNumber + 1, options.MaxRetries, args.RetryDelay, reason));
                    if (args.Outcome.Result is { } result && discard is not null)
                    {
                        await discard(result);
                    }
                }
            })
            .Build();
        return new PollyRetryPolicy<T>(pipeline);
    }
}
