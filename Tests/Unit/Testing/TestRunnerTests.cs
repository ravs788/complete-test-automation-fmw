using Core.Logging;
using Core.Testing;

namespace Tests.Unit.Testing;

public class TestRunnerTests
{
    private sealed class RecordingLogger : ILoggingService
    {
        public List<string> Lines { get; } = [];
        public void Configure(string indexFormat, string? username = null, string? password = null, string? elasticUrl = null) { }
        public void Info(string message, LogMetadata? metadata = null) => Lines.Add("INFO " + message);
        public void Error(string message, LogMetadata? metadata = null) => Lines.Add("ERROR " + message);
        public void Debug(string message, LogMetadata? metadata = null) => Lines.Add("DEBUG " + message);
        public void Shutdown() { }
    }

    private static TestRunInfo Info(RecordingLogger logger) => new(logger, "unit", typeof(TestRunnerTests), "Sample");

    [Fact]
    public async Task RunAsync_Should_RunCleanup_And_NotCallOnFailure_When_BodyPasses()
    {
        // Arrange
        var logger = new RecordingLogger();
        var failed = false;
        var cleaned = false;

        // Act
        await TestRunner.RunAsync(Info(logger), () => Task.CompletedTask,
            onFailureAsync: () => { failed = true; return Task.CompletedTask; },
            cleanupAsync: () => { cleaned = true; return Task.CompletedTask; });

        // Assert
        Assert.False(failed);
        Assert.True(cleaned);
        Assert.Contains(logger.Lines, l => l.StartsWith("INFO [Finish]") && l.Contains("Outcome: Passed"));
    }

    [Fact]
    public async Task RunAsync_Should_CallOnFailure_RunCleanup_And_Rethrow_When_BodyThrows()
    {
        // Arrange
        var logger = new RecordingLogger();
        var failed = false;
        var cleaned = false;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => TestRunner.RunAsync(Info(logger),
            () => throw new InvalidOperationException("boom"),
            onFailureAsync: () => { failed = true; return Task.CompletedTask; },
            cleanupAsync: () => { cleaned = true; return Task.CompletedTask; }));

        // Assert: failure details and lifecycle callbacks are preserved.
        Assert.Equal("boom", ex.Message);
        Assert.True(failed);
        Assert.True(cleaned);
        Assert.Contains(logger.Lines, l => l.StartsWith("ERROR [Failed]") && l.Contains("boom"));
    }

    [Fact]
    public async Task RunAsync_Should_KeepTheOriginalError_When_OnFailureThrows()
    {
        // Arrange: supply a failing body and a failure handler that also throws.
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => TestRunner.RunAsync(Info(new RecordingLogger()),
            () => throw new InvalidOperationException("original"),
            onFailureAsync: () => throw new IOException("screenshot failed")));

        // Assert: the body failure remains the reported error.
        Assert.Equal("original", ex.Message);
    }
}
