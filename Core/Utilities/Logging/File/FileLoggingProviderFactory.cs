using System;

namespace Core.Utilities
{
    /// <summary>
    /// Provider factory for file-backed ILoggingService.
    /// </summary>
    public class FileLoggingProviderFactory : ILoggingProviderFactory
    {
        public string Name => "file";

        public bool TryCreate(LoggingConfig cfg, string indexFormat, out ILoggingService service, out string? reason)
        {
            service = null!;
            reason = null;

            var fileCfg = cfg.FileLogging;
            if (fileCfg == null || !fileCfg.Enabled)
            {
                reason = "File logging disabled by configuration.";
                return false;
            }

            var fileSvc = new FileLoggingService(fileCfg);
            fileSvc.Configure(indexFormat);
            service = fileSvc;
            return true;
        }
    }
}
