using System;

namespace Core.Utilities
{
    /// <summary>
    /// Results publisher that emits a concise summary to the console.
    /// </summary>
    public class ConsoleResultsPublisher : IResultsPublisher
    {
        public void Publish(LogMetadata metadata, string? overrideIndexName = null)
        {
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));
            System.Console.WriteLine($"[Results] Provider=console | Project={metadata.ProjectName} | Class={metadata.TestClassName} | Method={metadata.TestMethodName} | Status={metadata.Status} | Duration={metadata.Duration}s");
        }
    }
}
