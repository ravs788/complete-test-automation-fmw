using Core.Testing;

namespace Tests.Unit.Testing;

public class CleanupTrackerTests
{
    [Fact]
    public async Task RunAsync_Should_RunCleanupsInReverseOrder_And_Empty()
    {
        // Arrange
        var order = new List<string>();
        var tracker = new CleanupTracker();
        tracker.Add("first", () => { order.Add("first"); return Task.CompletedTask; });
        tracker.Add("second", () => { order.Add("second"); return Task.CompletedTask; });

        // Act
        await tracker.RunAsync();

        // Assert
        Assert.Equal(["second", "first"], order);
        Assert.Equal(0, tracker.Count);
    }

    [Fact]
    public async Task RunAsync_Should_ReportFailures_And_ContinueWithTheRest()
    {
        // Arrange
        var ran = new List<string>();
        var errors = new List<string>();
        var tracker = new CleanupTracker();
        tracker.Add("delete row 1", () => { ran.Add("row 1"); return Task.CompletedTask; });
        tracker.Add("delete row 2", () => throw new InvalidOperationException("db down"));

        // Act
        await tracker.RunAsync(errors.Add);

        // Assert
        Assert.Equal(["row 1"], ran);
        Assert.Equal(["[Cleanup] delete row 2 failed: db down"], errors);
    }
}
