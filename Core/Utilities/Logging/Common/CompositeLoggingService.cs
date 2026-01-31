using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utilities
{
    /// <summary>
    /// A simple fan-out logger that forwards calls to multiple ILoggingService instances.
    /// Used to support "also write to file" alongside an existing provider.
    /// </summary>
    public class CompositeLoggingService : ILoggingService
    {
        private readonly ILoggingService[] _services;

        public CompositeLoggingService(IEnumerable<ILoggingService> services)
        {
            _services = services?.Where(s => s != null).ToArray() ?? Array.Empty<ILoggingService>();
        }

        public void Configure(string indexFormat, string? username = null, string? password = null, string? elasticUrl = null)
        {
            foreach (var s in _services)
            {
                try { s.Configure(indexFormat, username, password, elasticUrl); }
                catch { /* swallow to avoid blocking others */ }
            }
        }

        public void Info(string message, LogMetadata? metadata = null)
        {
            foreach (var s in _services)
            {
                try { s.Info(message, metadata); }
                catch { /* swallow to avoid blocking others */ }
            }
        }

        public void Error(string message, LogMetadata? metadata = null)
        {
            foreach (var s in _services)
            {
                try { s.Error(message, metadata); }
                catch { /* swallow to avoid blocking others */ }
            }
        }

        public void Debug(string message, LogMetadata? metadata = null)
        {
            foreach (var s in _services)
            {
                try { s.Debug(message, metadata); }
                catch { /* swallow to avoid blocking others */ }
            }
        }

        public void Shutdown()
        {
            foreach (var s in _services)
            {
                try { s.Shutdown(); }
                catch { /* ignore */ }
            }
        }
    }
}
