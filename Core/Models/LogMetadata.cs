namespace Core.Utilities
{
    public class LogMetadata
    {
        public string ProjectName { get; set; }
        public string TestClassName { get; set; }
        public string TestMethodName { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
        public string Reason { get; set; }
        public string RunTime { get; set; }
        public string RunName { get; set; }
        public string TriggeredBy { get; set; }
        public string Browser { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
