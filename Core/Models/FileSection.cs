namespace Core.Utilities
{
    public class FileSection
    {
        // Enable/disable the file logging sink
        public bool Enabled { get; set; } = true;

        // Directory to write logs into. Relative paths are resolved against the solution root.
        public string DirectoryPath { get; set; } = "logs";

        // Template for log file name. Supports:
        //  - {index} -> resolved index name (from indexFormat)
        //  - {date:FORMAT} -> UTC now formatted with .NET date format, e.g. {date:yyyy-MM-dd}
        public string FileNameTemplate { get; set; } = "{index}-{date:yyyy-MM-dd}.log";

        // Rolling strategy: "date" (default) or "size"
        // - date: uses {date:...} token in FileNameTemplate to roll daily
        // - size: keeps a single file name and rolls with .1, .2 suffix when file exceeds MaxFileSizeMB
        public string RollingStrategy { get; set; } = "date";

        // Max file size in megabytes for size-based rolling
        public int MaxFileSizeMB { get; set; } = 10;

        // Number of days to keep log files (older files are deleted on startup/write).
        // Set to 0 or negative to disable cleanup.
        public int RetentionDays { get; set; } = 14;

        // If true, write to file in addition to the selected primary provider (elastic/console/etc.).
        public bool AlsoWriteToFile { get; set; } = false;
    }
}
