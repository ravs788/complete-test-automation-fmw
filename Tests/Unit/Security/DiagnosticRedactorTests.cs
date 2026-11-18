using Core.Api;
using Core.Logging;
using Core.Security;
using Core.Testing;

namespace Tests.Unit.Security;

public sealed class DiagnosticRedactorTests
{
    [Theory]
    [InlineData("GET https://user:canary-userinfo@example.invalid/items?access_token=canary-query&sort=canary-sort#canary-fragment failed")]
    [InlineData("GET /items?token=canary-relative failed")]
    [InlineData("Authorization: Bearer canary-header")]
    [InlineData("Cookie: session=canary-cookie; token=canary-token")]
    [InlineData("{\"password\":\"canary with spaces\",\"token\":\"canary-json\"}")]
    [InlineData("password='canary quoted value'")]
    public void Redact_Should_RemoveSensitiveValues_And_BeIdempotent(string input)
    {
        // Arrange: the theory supplies sensitive diagnostic text.
        // Act
        var result = DiagnosticRedactor.Redact(input);

        // Assert
        Assert.DoesNotContain("canary", result, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(result, DiagnosticRedactor.Redact(result));
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Redact_Should_PreserveUsefulNonSensitiveContext()
    {
        // Arrange
        const string text = "PUT booking/1 failed: 403 Forbidden";

        // Act & Assert
        Assert.Equal(text, DiagnosticRedactor.Redact(text));
    }

    [Fact]
    public void RegisteredSecret_Should_BeRemovedEvenWithoutAFieldName()
    {
        // Arrange
        var secret = $"opaque-{Guid.NewGuid():N}/value";
        DiagnosticRedactor.RegisterSecret(secret);

        // Act
        var result = DiagnosticRedactor.Redact($"failure {secret} encoded {Uri.EscapeDataString(secret)}");

        // Assert
        Assert.DoesNotContain(secret, result);
        Assert.DoesNotContain(Uri.EscapeDataString(secret), result);
    }

    [Fact]
    public void ApiException_Should_NotExposeSensitiveUrl_InToString()
    {
        // Arrange: use a synthetic URL containing credentials and a token.
        // Act
        var error = new ApiException("GET", "https://user:canary-password@example.invalid/items?token=canary-token", 403, "Forbidden");

        // Assert
        Assert.DoesNotContain("canary", error.ToString());
        Assert.Contains("items", error.Message);
        Assert.Equal(403, error.StatusCode);
    }

    [Fact]
    public async Task Runner_Should_SanitizeBothSinkMessagesAndTheHostFailure_WithoutKeepingRawInnerException()
    {
        // Arrange
        var logger = new RecordingLogger();
        var original = new IOException("request https://user:canary-password@example.invalid/path?token=canary-token failed");
        var cleanup = false;

        // Act & Assert
        var error = await Assert.ThrowsAsync<RedactedTestException>(() => TestRunner.RunAsync(
            new TestRunInfo(logger, "security", typeof(DiagnosticRedactorTests), "synthetic"),
            () => throw original, onFailureAsync: () => Task.CompletedTask,
            cleanupAsync: () => { cleanup = true; return Task.CompletedTask; }));

        // Assert: only sanitized failure details reach the host and logger, and cleanup ran.
        Assert.DoesNotContain("canary", error.ToString());
        Assert.Null(error.InnerException);
        Assert.Contains(nameof(IOException), error.Message);
        Assert.DoesNotContain(logger.Messages, m => m.Contains("canary", StringComparison.Ordinal));
        Assert.True(cleanup);
    }

    [Fact]
    public void Metadata_Should_RedactEveryFreeTextField_WithoutChangingTheOriginal()
    {
        // Arrange
        var original = new LogMetadata { Reason = "token=canary-token", TestMethodName = "https://u:canary-secret@example.invalid/test" };

        // Act
        var result = original.Redacted();

        // Assert
        Assert.DoesNotContain("canary", result.Reason + result.TestMethodName);
        Assert.Contains("canary", original.Reason);
    }

    private sealed class RecordingLogger : ILoggingService
    {
        public List<string> Messages { get; } = [];
        public void Configure(string indexFormat, string? username = null, string? password = null, string? elasticUrl = null) { }
        public void Info(string message, LogMetadata? metadata = null) => Messages.Add(message);
        public void Error(string message, LogMetadata? metadata = null) => Messages.Add(message);
        public void Debug(string message, LogMetadata? metadata = null) => Messages.Add(message);
        public void Shutdown() { }
    }
}
