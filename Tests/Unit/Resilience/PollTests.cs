using Core.Resilience;

namespace Tests.Unit.Resilience;

public class PollTests
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(10);

    [Fact]
    public async Task UntilAsync_Should_Return_When_ConditionBecomesTrue()
    {
        // Arrange
        var checks = 0;

        // Act
        await Poll.UntilAsync(() => Task.FromResult(++checks == 3), TimeSpan.FromSeconds(5), Short);

        // Assert
        Assert.Equal(3, checks);
    }

    [Fact]
    public async Task UntilAsync_Should_ThrowTimeout_With_TheDescription()
    {
        // Arrange: use a condition that never becomes true and a short timeout.
        // Act & Assert
        var ex = await Assert.ThrowsAsync<TimeoutException>(() =>
            Poll.UntilAsync(() => Task.FromResult(false), TimeSpan.FromMilliseconds(50), Short, "booking to appear"));

        // Assert: the timeout includes the wait description.
        Assert.Contains("booking to appear", ex.Message);
    }

    [Fact]
    public async Task UntilAsync_Should_ReturnTheReadyValue()
    {
        // Arrange
        var values = new Queue<string>(["pending", "pending", "done"]);

        // Act
        var value = await Poll.UntilAsync(() => Task.FromResult(values.Dequeue()), v => v == "done", TimeSpan.FromSeconds(5), Short);

        // Assert
        Assert.Equal("done", value);
    }
}
