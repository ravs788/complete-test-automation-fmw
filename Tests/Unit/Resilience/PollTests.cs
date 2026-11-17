using Core.Resilience;

namespace Tests.Unit.Resilience;

public class PollTests
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(10);

    [Fact]
    public async Task UntilAsync_Should_Return_When_ConditionBecomesTrue()
    {
        var checks = 0;

        await Poll.UntilAsync(() => Task.FromResult(++checks == 3), TimeSpan.FromSeconds(5), Short);

        Assert.Equal(3, checks);
    }

    [Fact]
    public async Task UntilAsync_Should_ThrowTimeout_With_TheDescription()
    {
        var ex = await Assert.ThrowsAsync<TimeoutException>(() =>
            Poll.UntilAsync(() => Task.FromResult(false), TimeSpan.FromMilliseconds(50), Short, "booking to appear"));

        Assert.Contains("booking to appear", ex.Message);
    }

    [Fact]
    public async Task UntilAsync_Should_ReturnTheReadyValue()
    {
        var values = new Queue<string>(["pending", "pending", "done"]);

        var value = await Poll.UntilAsync(() => Task.FromResult(values.Dequeue()), v => v == "done", TimeSpan.FromSeconds(5), Short);

        Assert.Equal("done", value);
    }
}
