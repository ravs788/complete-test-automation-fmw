using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Utilities
{
    /// <summary>
    /// Generic selector for logging providers. Uses pluggable factories (ILoggingProviderFactory)
    /// discovered via reflection, so no direct dependency on any specific provider.
    /// </summary>
    public static class LoggingServiceFactory
    {
        /// <summary>
        /// Creates and configures an ILoggingService using the configured provider.
        /// Falls back to another available provider (preferably "console") if the chosen one is unavailable.
        /// </summary>
        public static ILoggingService CreateLogger(string indexFormat)
        {
            var cfg = LoggingConfig.Load();
            var factories = DiscoverFactories();

            // Try explicitly selected provider first
            var provider = (cfg.Provider ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(provider))
            {
                var selected = factories.FirstOrDefault(f =>
                    string.Equals(f.Name, provider, StringComparison.OrdinalIgnoreCase));

                if (selected != null && selected.TryCreate(cfg, indexFormat, out var svc, out _))
                {
                    return MaybeWrapWithFile(cfg, indexFormat, svc);
                }
            }

            // Prefer console if available
            var console = factories.FirstOrDefault(f =>
                string.Equals(f.Name, "console", StringComparison.OrdinalIgnoreCase));
            if (console != null && console.TryCreate(cfg, indexFormat, out var consoleSvc, out _))
            {
                return MaybeWrapWithFile(cfg, indexFormat, consoleSvc);
            }

            // Otherwise, try any available provider
            foreach (var f in factories)
            {
                if (f.TryCreate(cfg, indexFormat, out var svc, out _))
                {
                    return MaybeWrapWithFile(cfg, indexFormat, svc);
                }
            }

            // Absolute last resort: direct console logger (in case no factory type was discovered)
            var fallback = new ConsoleLoggingService();
            fallback.Configure(indexFormat);
            return MaybeWrapWithFile(cfg, indexFormat, fallback);
        }

        private static ILoggingService MaybeWrapWithFile(LoggingConfig cfg, string indexFormat, ILoggingService primary)
        {
            try
            {
                var fileCfg = cfg.FileLogging;
                if (fileCfg != null && fileCfg.Enabled && fileCfg.AlsoWriteToFile)
                {
                    var fileSvc = new FileLoggingService(fileCfg);
                    fileSvc.Configure(indexFormat);
                    return new CompositeLoggingService(new[] { primary, fileSvc });
                }
            }
            catch
            {
                // ignore wrapping failures
            }
            return primary;
        }

        private static List<ILoggingProviderFactory> DiscoverFactories()
        {
            var asm = typeof(ILoggingProviderFactory).Assembly;
            var types = asm
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(ILoggingProviderFactory).IsAssignableFrom(t));

            var list = new List<ILoggingProviderFactory>();
            foreach (var t in types)
            {
                try
                {
                    if (Activator.CreateInstance(t) is ILoggingProviderFactory instance)
                        list.Add(instance);
                }
                catch
                {
                    // ignore load/activation failures
                }
            }

            return list;
        }
    }
}
