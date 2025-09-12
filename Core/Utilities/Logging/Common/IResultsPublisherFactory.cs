using System;

namespace Core.Utilities
{
    /// <summary>
    /// Pluggable factory contract for creating results publishers without coupling
    /// the generic selection logic to any specific implementation.
    /// </summary>
    public interface IResultsPublisherFactory
    {
        /// <summary>A unique, case-insensitive provider name, e.g. "elastic", "console".</summary>
        string Name { get; }

        /// <summary>
        /// Attempts to create a publisher for the given config.
        /// Return true and set 'publisher' when ready for use; return false to indicate
        /// this provider is unavailable (e.g., server unreachable) so the caller can fallback.
        /// </summary>
        bool TryCreate(LoggingConfig cfg, out IResultsPublisher publisher, out string? reason);
    }
}
