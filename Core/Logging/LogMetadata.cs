using Core.Security;

namespace Core.Logging;

public sealed record LogMetadata
{
    public string ProjectName { get; init; } = string.Empty;
    public string TestClassName { get; init; } = string.Empty;
    public string TestMethodName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string RunTime { get; init; } = string.Empty;
    public string RunName { get; init; } = string.Empty;
    public string TriggeredBy { get; init; } = string.Empty;
    public string Browser { get; init; } = string.Empty;
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    /// <summary>A copy safe for framework diagnostic sinks.</summary>
    public LogMetadata Redacted() => this with
    {
        ProjectName = DiagnosticRedactor.Redact(ProjectName),
        TestClassName = DiagnosticRedactor.Redact(TestClassName),
        TestMethodName = DiagnosticRedactor.Redact(TestMethodName),
        Status = DiagnosticRedactor.Redact(Status),
        Duration = DiagnosticRedactor.Redact(Duration),
        Reason = DiagnosticRedactor.Redact(Reason),
        RunTime = DiagnosticRedactor.Redact(RunTime),
        RunName = DiagnosticRedactor.Redact(RunName),
        TriggeredBy = DiagnosticRedactor.Redact(TriggeredBy),
        Browser = DiagnosticRedactor.Redact(Browser),
    };
}
