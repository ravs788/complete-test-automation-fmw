using System;

namespace Core.Utilities
{
    /// <summary>
    /// Provider factory for the console logger. Always available.
    /// </summary>
    public class ConsoleLoggingProviderFactory : ILoggingProviderFactory
    {
        public string Name => "console";

        public bool TryCreate(LoggingConfig cfg, string indexFormat, out ILoggingService service, out string? reason)
        {
            var console = new ConsoleLoggingService();
            console.Configure(indexFormat);
            service = console;
            reason = null;
            return true;
        }
    }
}
