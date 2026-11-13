using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utilities
{
    /// <summary>
    /// Generic selector for results publishers. Uses pluggable factories (IResultsPublisherFactory)
    /// discovered via reflection, so no direct dependency on any specific provider.
    /// </summary>
    public static class ResultsPublisherFactory
    {
        /// <summary>
        /// Creates an IResultsPublisher using the configured provider.
        /// Falls back to another available provider (preferably "console") if the chosen one is unavailable.
        /// </summary>
        public static IResultsPublisher Create()
        {
            var cfg = LoggingConfig.Load();
            var factories = DiscoverFactories();

            // Try explicitly selected provider first
            var provider = (cfg.Provider ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(provider))
            {
                var selected = factories.FirstOrDefault(f =>
                    string.Equals(f.Name, provider, StringComparison.OrdinalIgnoreCase));

                if (selected != null && selected.TryCreate(cfg, out var pub, out _))
                {
                    return pub;
                }
            }

            // Prefer console if available
            var console = factories.FirstOrDefault(f =>
                string.Equals(f.Name, "console", StringComparison.OrdinalIgnoreCase));
            if (console != null && console.TryCreate(cfg, out var consolePub, out _))
            {
                return consolePub;
            }

            // Otherwise, try any available provider
            foreach (var f in factories)
            {
                if (f.TryCreate(cfg, out var pub, out _))
                {
                    return pub;
                }
            }

            // Absolute last resort: inline console publisher (in case no factory type was discovered)
            return new InlineConsoleResultsPublisher();
        }

        private static List<IResultsPublisherFactory> DiscoverFactories()
        {
            var asm = typeof(IResultsPublisherFactory).Assembly;
            var types = asm
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(IResultsPublisherFactory).IsAssignableFrom(t));

            var list = new List<IResultsPublisherFactory>();
            foreach (var t in types)
            {
                try
                {
                    if (Activator.CreateInstance(t) is IResultsPublisherFactory instance)
                        list.Add(instance);
                }
                catch
                {
                    // ignore load/activation failures
                }
            }

            return list;
        }

        /// <summary>Minimal console publisher used as an emergency fallback.</summary>
        private class InlineConsoleResultsPublisher : IResultsPublisher
        {
            public void Publish(LogMetadata metadata, string? overrideIndexName = null)
            {
                if (metadata is null) throw new ArgumentNullException(nameof(metadata));
                System.Console.WriteLine($"[Results] Provider=console | Project={metadata.ProjectName} | Class={metadata.TestClassName} | Method={metadata.TestMethodName} | Status={metadata.Status} | Duration={metadata.Duration}s");
            }
        }
    }
}
