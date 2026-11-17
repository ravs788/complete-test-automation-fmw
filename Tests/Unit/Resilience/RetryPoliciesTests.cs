using Core.Resilience;
using Core.Resilience.Polly;

namespace Tests.Unit.Resilience;

/// <summary>
/// The behaviour every <see cref="IRetryPolicyFactory"/> must have (R45, R46), through the framework's named policies.
/// Each implementation's test class inherits these tests.
/// </summary>
public abstract class RetryPolicyContractTests
{
    private static readonly RetryOptions Fast = new() { MaxRetries = 2, BaseDelayMs = 1 };

    protected abstract IRetryPolicyFactory Factory { get; }

    [Fact]
    public async Task TransientHttp_Should_RetryGatewayErrors_And_DiscardRetriedResponses()
    {
        var statuses = new Queue<int>([503, 502, 200]);
        var calls = 0;
        var discarded = new List<int>();
        var log = new List<string>();
        var policy = RetryPolicies.TransientHttp<int>(Fast, s => s, s => { discarded.Add(s); return ValueTask.CompletedTask; }, log.Add, Factory);

        var result = await policy.ExecuteAsync(_ => { calls++; return Task.FromResult(statuses.Dequeue()); });

        Assert.Equal(200, result);
        Assert.Equal(3, calls);
        Assert.Equal([503, 502], discarded);
        Assert.Equal(2, log.Count);
        Assert.StartsWith("[Retry] attempt 1 of 2 after ", log[0]);
        Assert.EndsWith("ms: HTTP 503", log[0]);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(404)]
    [InlineData(201)]
    public async Task TransientHttp_Should_NotRetryOtherStatuses(int status)
    {
        var calls = 0;
        var policy = RetryPolicies.TransientHttp<int>(Fast, s => s, factory: Factory);

        var result = await policy.ExecuteAsync(_ => { calls++; return Task.FromResult(status); });

        Assert.Equal(status, result);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task TransientHttp_Should_ReturnTheLastResult_When_RetriesAreExhausted()
    {
        var calls = 0;
        var policy = RetryPolicies.TransientHttp<int>(Fast, s => s, factory: Factory);

        var result = await policy.ExecuteAsync(_ => { calls++; return Task.FromResult(504); });

        Assert.Equal(504, result);
        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task TransientHttp_Should_RetryNetworkErrors_ButNotOtherErrors()
    {
        var calls = 0;
        var policy = RetryPolicies.TransientHttp<int>(Fast, s => s, factory: Factory);
        var result = await policy.ExecuteAsync(_ =>
            ++calls < 2 ? throw new HttpRequestException("connect ECONNRESET 1.2.3.4:443") : Task.FromResult(200));
        Assert.Equal((200, 2), (result, calls));

        calls = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync(_ => { calls++; throw new InvalidOperationException("bug"); }));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Navigation_Should_RetryOnlyBrowserNetworkErrors()
    {
        var calls = 0;
        var policy = RetryPolicies.Navigation(Fast, factory: Factory);
        await policy.ExecuteAsync(_ =>
            ++calls < 3 ? throw new InvalidOperationException("net::ERR_CONNECTION_RESET at https://example.test/") : Task.CompletedTask);
        Assert.Equal(3, calls);

        calls = 0;
        await Assert.ThrowsAsync<TimeoutException>(() =>
            policy.ExecuteAsync(_ => { calls++; throw new TimeoutException("Timeout 30000ms exceeded."); }));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Navigation_Should_ReturnTheActionsResult()
    {
        var policy = RetryPolicies.Navigation(Fast, factory: Factory);

        Assert.Equal("page", await policy.ExecuteAsync(_ => Task.FromResult("page")));
    }

    [Fact]
    public void Transient_Should_RetrySyncActions_And_GiveUpAfterMaxRetries()
    {
        var calls = 0;
        var policy = RetryPolicies.Transient(Fast, ex => ex is TimeoutException, factory: Factory);

        Assert.Throws<TimeoutException>(() => policy.Execute(() => { calls++; throw new TimeoutException(); }));
        Assert.Equal(3, calls);
        Assert.Equal(7, policy.Execute(() => 7));
    }

    [Fact]
    public void Transient_Should_NotRetry_When_MaxRetriesIsZero()
    {
        var calls = 0;
        var policy = RetryPolicies.Transient(new RetryOptions { MaxRetries = 0 }, _ => true, factory: Factory);

        Assert.Throws<TimeoutException>(() => policy.Execute(() => { calls++; throw new TimeoutException(); }));
        Assert.Equal(1, calls);
    }
}

public sealed class BuiltInRetryPolicyContractTests : RetryPolicyContractTests
{
    protected override IRetryPolicyFactory Factory { get; } = new BuiltInRetryPolicyFactory();
}

public sealed class PollyRetryPolicyContractTests : RetryPolicyContractTests
{
    protected override IRetryPolicyFactory Factory { get; } = new PollyRetryPolicyFactory();
}

public class RetryPoliciesTests
{
    [Theory]
    [InlineData("GET", true)]
    [InlineData("put", true)]
    [InlineData("DELETE", true)]
    [InlineData("HEAD", true)]
    [InlineData("POST", false)]
    [InlineData("PATCH", false)]
    public void IsIdempotent_Should_AllowOnlySafeToRepeatMethods(string method, bool expected)
    {
        Assert.Equal(expected, RetryPolicies.IsIdempotent(method));
    }

    [Theory]
    [InlineData("connect ECONNRESET 1.2.3.4:443", true)]
    [InlineData("net::ERR_NAME_NOT_RESOLVED", true)]
    [InlineData("Timeout 30000ms exceeded.", false)]
    public void IsNetworkError_Should_RecogniseNetworkFailures_WhateverThrewThem(string message, bool expected)
    {
        Assert.Equal(expected, RetryPolicies.IsNetworkError(new InvalidOperationException(message)));
        Assert.True(RetryPolicies.IsNetworkError(new HttpRequestException("anything")));
    }
}
