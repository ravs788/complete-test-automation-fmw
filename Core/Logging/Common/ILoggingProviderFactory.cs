using System;

namespace Core.Utilities
{
    /// <summary>
    /// Pluggable factory contract for creating logging providers without coupling
    /// the generic selection logic to any specific implementation.
    /// </summary>
    public interface ILoggingProviderFactory
    {
        /// <summary>A unique, case-insensitive provider name, e.g. "elastic", "console".</summary>
        string Name { get; }

        /// <summary>
        /// Attempts to create a configured logger for the given config/indexFormat.
        /// Return true and set 'service' when ready for use; return false to indicate
        /// this provider is unavailable (e.g., server unreachable) so the caller can fallback.
        /// </summary>
        bool TryCreate(LoggingConfig cfg, string indexFormat, out ILoggingService service, out string? reason);
    }
}
