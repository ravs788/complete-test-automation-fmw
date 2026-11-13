namespace Core.Utilities
{
    public interface ILoggingService
    {
        void Configure(string indexFormat, string? username = null, string? password = null, string? elasticUrl = null);

        void Info(string message, LogMetadata? metadata = null);

        void Error(string message, LogMetadata? metadata = null);

        void Debug(string message, LogMetadata? metadata = null);

        void Shutdown();
    }
}
