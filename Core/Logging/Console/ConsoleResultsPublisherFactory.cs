using System;

namespace Core.Utilities
{
    /// <summary>
    /// Factory for console results publisher.
    /// </summary>
    public class ConsoleResultsPublisherFactory : IResultsPublisherFactory
    {
        public string Name => "console";

        public bool TryCreate(LoggingConfig cfg, out IResultsPublisher publisher, out string? reason)
        {
            publisher = new ConsoleResultsPublisher();
            reason = null;
            return true;
        }
    }
}
