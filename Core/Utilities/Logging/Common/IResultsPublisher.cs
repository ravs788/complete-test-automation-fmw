using System;

namespace Core.Utilities
{
    /// <summary>
    /// Abstraction for publishing test execution metadata (e.g., to Elastic, console, etc.).
    /// </summary>
    public interface IResultsPublisher
    {
        /// <summary>
        /// Publish the provided metadata. Implementations decide the destination and format.
        /// </summary>
        void Publish(LogMetadata metadata, string? overrideIndexName = null);
    }
}
