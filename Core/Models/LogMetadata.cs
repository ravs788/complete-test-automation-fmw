namespace Core.Utilities
{
    public record class LogMetadata
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
    }
}
