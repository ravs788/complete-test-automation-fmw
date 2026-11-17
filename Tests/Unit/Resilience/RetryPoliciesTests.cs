using Core.Resilience;
using Microsoft.Playwright;

namespace Tests.Unit.Resilience;

public class RetryPoliciesTests
{
    private static readonly RetryOptions Fast = new() { MaxRetries = 2, BaseDelayMs = 1 };

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

    [Fact]
    public async Task TransientHttp_Should_RetryGatewayErrors_And_DiscardRetriedResponses()
    {
        var statuses = new Queue<int>([503, 502, 200]);
        var calls = 0;
        var discarded = new List<int>();
        var log = new List<string>();
        var pipeline = RetryPolicies.TransientHttp<int>(Fast, s => s, s => { discarded.Add(s); return ValueTask.CompletedTask; }, log.Add);

        var result = await pipeline.ExecuteAsync(_ => { calls++; return ValueTask.FromResult(statuses.Dequeue()); });

        Assert.Equal(200, result);
        Assert.Equal(3, calls);
        Assert.Equal([503, 502], discarded);
        Assert.Equal(2, log.Count);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(404)]
    [InlineData(201)]
    public async Task TransientHttp_Should_NotRetryOtherStatuses(int status)
    {
        var calls = 0;
        var pipeline = RetryPolicies.TransientHttp<int>(Fast, s => s);

        var result = await pipeline.ExecuteAsync(_ => { calls++; return ValueTask.FromResult(status); });

        Assert.Equal(status, result);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task TransientHttp_Should_ReturnTheLastResult_When_RetriesAreExhausted()
    {
        var calls = 0;
        var pipeline = RetryPolicies.TransientHttp<int>(Fast, s => s);

        var result = await pipeline.ExecuteAsync(_ => { calls++; return ValueTask.FromResult(504); });

        Assert.Equal(504, result);
        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task TransientHttp_Should_RetryNetworkErrors_ButNotOtherErrors()
    {
        var calls = 0;
        var pipeline = RetryPolicies.TransientHttp<int>(Fast, s => s);
        var result = await pipeline.ExecuteAsync(_ =>
            ++calls < 2 ? throw new PlaywrightException("connect ECONNRESET 1.2.3.4:443") : ValueTask.FromResult(200));
        Assert.Equal((200, 2), (result, calls));

        calls = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await pipeline.ExecuteAsync<int>(_ => { calls++; throw new InvalidOperationException("bug"); }));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Navigation_Should_RetryOnlyBrowserNetworkErrors()
    {
        var calls = 0;
        var pipeline = RetryPolicies.Navigation(Fast);
        await pipeline.ExecuteAsync(_ =>
            ++calls < 3 ? throw new PlaywrightException("net::ERR_CONNECTION_RESET at https://example.test/") : ValueTask.CompletedTask);
        Assert.Equal(3, calls);

        calls = 0;
        await Assert.ThrowsAsync<PlaywrightException>(async () =>
            await pipeline.ExecuteAsync(_ => { calls++; throw new PlaywrightException("Timeout 30000ms exceeded."); }));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Transient_Should_RetrySyncActions_And_GiveUpAfterMaxRetries()
    {
        var calls = 0;
        var pipeline = RetryPolicies.Transient(Fast, ex => ex is TimeoutException);

        Assert.Throws<TimeoutException>(() => pipeline.Execute(() => { calls++; throw new TimeoutException(); }));
        Assert.Equal(3, calls);
    }

    [Fact]
    public void Transient_Should_NotRetry_When_MaxRetriesIsZero()
    {
        var calls = 0;
        var pipeline = RetryPolicies.Transient(new RetryOptions { MaxRetries = 0 }, _ => true);

        Assert.Throws<TimeoutException>(() => pipeline.Execute(() => { calls++; throw new TimeoutException(); }));
        Assert.Equal(1, calls);
    }
}
