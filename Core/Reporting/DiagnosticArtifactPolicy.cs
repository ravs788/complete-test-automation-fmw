namespace Core.Reporting;

/// <summary>Controls optional diagnostic captures and attachments, independently of test pass/fail reporting.</summary>
public sealed record DiagnosticArtifactPolicy(bool AllowAttachments)
{
    /// <summary>Does not invoke capture at all when restricted, so sensitive bytes are never collected here.</summary>
    public async Task CaptureAsync(Func<Task<byte[]>> capture, Action<byte[]> attach)
    {
        if (!AllowAttachments) return;
        attach(await capture());
    }

    public static DiagnosticArtifactPolicy Current => FromMode(Environment.GetEnvironmentVariable("DIAGNOSTIC_ARTIFACTS"));

    public static DiagnosticArtifactPolicy FromMode(string? mode) => mode?.Trim().ToLowerInvariant() switch
    {
        null or "" or "standard" => new(true),
        "restricted" => new(false),
        _ => throw new InvalidOperationException("DIAGNOSTIC_ARTIFACTS must be standard or restricted.")
    };
}
