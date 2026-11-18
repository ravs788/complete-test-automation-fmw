using Core.Reporting;

namespace Tests.Unit.Reporting;

public sealed class DiagnosticArtifactPolicyTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("standard", true)]
    [InlineData("restricted", false)]
    [InlineData(" RESTRICTED ", false)]
    public async Task Policy_Should_ControlCaptureAndPublication(string? mode, bool expected)
    {
        // Arrange
        var captured = false;
        byte[]? attached = null;
        byte[] syntheticSensitiveContent = [1, 2, 3];

        // Act
        await DiagnosticArtifactPolicy.FromMode(mode).CaptureAsync(
            () => { captured = true; return Task.FromResult(syntheticSensitiveContent); },
            bytes => attached = bytes);

        // Assert
        Assert.Equal(expected, captured);
        if (expected) Assert.Same(syntheticSensitiveContent, attached);
        else Assert.Null(attached);
    }

    [Fact]
    public void InvalidMode_Should_FailInsteadOfEnablingCapture()
    {
        // Arrange: use an unrecognized artifact mode.
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => DiagnosticArtifactPolicy.FromMode("restriced"));
    }
}
