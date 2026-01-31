using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Core.Utilities
{
    /// <summary>
    /// File-backed implementation of ILoggingService.
    /// Supports daily rolling via filename template and basic retention by days.
    /// </summary>
    public class FileLoggingService : ILoggingService
    {
        private readonly FileSection _settings;
        private readonly object _sync = new object();

        private string _indexFormat = "logs-default";
        private string _rootPath = "";
        private string _resolvedDir = "";
        private string _currentFilePath = "";
        private DateTime _currentDateUtc = DateTime.MinValue;
        private bool _initialized = false;

        // Size-based rolling
        private bool _sizeRolling = false;
        private long _maxBytes = 10L * 1024L * 1024L; // default 10 MB

        public FileLoggingService(FileSection settings)
        {
            _settings = settings ?? new FileSection();
        }

        public void Configure(string indexFormat, string? username = null, string? password = null, string? elasticUrl = null)
        {
            _indexFormat = string.IsNullOrWhiteSpace(indexFormat) ? "logs-default" : indexFormat;
            _rootPath = FindRootPath();
            _resolvedDir = ResolveDirectory(_rootPath, _settings.DirectoryPath);

            // rolling mode selection
            _sizeRolling = string.Equals(_settings.RollingStrategy, "size", StringComparison.OrdinalIgnoreCase);
            var mb = _settings.MaxFileSizeMB <= 0 ? 10 : _settings.MaxFileSizeMB;
            _maxBytes = (long)mb * 1024L * 1024L;
            _currentFilePath = string.Empty; // recompute on first write

            EnsureDirectory();
            ApplyRetentionCleanup();
            _initialized = true;
        }

        public void Info(string message, LogMetadata? metadata = null) =>
            Write("INFO", message, metadata);

        public void Error(string message, LogMetadata? metadata = null) =>
            Write("ERROR", message, metadata);

        public void Debug(string message, LogMetadata? metadata = null) =>
            Write("DEBUG", message, metadata);

        private void Write(string level, string message, LogMetadata? metadata)
        {
            try
            {
                var nowUtc = DateTime.UtcNow;
                var indexName = ResolveIndexName(nowUtc);

                var line = $"[{nowUtc:o}] [{level}] [{indexName}] {message}";
                if (metadata != null)
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = false });
                        line += $" | metadata={json}";
                    }
                    catch
                    {
                        // ignore metadata serialization issues for file logging
                    }
                }

                lock (_sync)
                {
                    if (!_initialized)
                    {
                        Configure(_indexFormat);
                    }

                    // Determine/roll file path
                    if (string.IsNullOrEmpty(_currentFilePath))
                    {
                        _currentFilePath = _sizeRolling
                            ? ResolveSizeFilePath(indexName, nowUtc)
                            : ResolveFilePath(indexName, nowUtc);
                        _currentDateUtc = nowUtc.Date;
                    }

                    if (_sizeRolling)
                    {
                        EnsureSizeRoll(_currentFilePath);
                    }
                    else if (_currentDateUtc.Date != nowUtc.Date)
                    {
                        _currentDateUtc = nowUtc.Date;
                        _currentFilePath = ResolveFilePath(indexName, nowUtc);
                    }

                    File.AppendAllText(_currentFilePath, line + Environment.NewLine);
                }
            }
            catch
            {
                // Swallow to avoid breaking test execution on I/O issues
            }
        }

        private string ResolveIndexName(DateTime utcNow)
        {
            try
            {
                var formatted = string.Format(_indexFormat, utcNow);
                return formatted.ToLowerInvariant();
            }
            catch (FormatException)
            {
                return _indexFormat.ToLowerInvariant();
            }
        }

        private string ResolveFilePath(string indexName, DateTime utcNow)
        {
            // Supports tokens:
            //  - {index}
            //  - {date:FORMAT}
            var fileName = _settings.FileNameTemplate ?? "{index}-{date:yyyy-MM-dd}.log";

            fileName = fileName.Replace("{index}", indexName);

            fileName = System.Text.RegularExpressions.Regex.Replace(
                fileName,
                "\\{date:(?<fmt>[^}]+)\\}",
                m =>
                {
                    var fmt = m.Groups["fmt"].Value;
                    try { return utcNow.ToString(fmt); } catch { return utcNow.ToString("yyyy-MM-dd"); }
                });

            // If no token used, still ensure .log extension
            if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            {
                fileName += ".log";
            }

            return Path.Combine(_resolvedDir, fileName);
        }

        // Size-based: use a stable file name (strip any {date:...} tokens) and roll by renaming base.log -> base.log.N
        private string ResolveSizeFilePath(string indexName, DateTime utcNow)
        {
            var fileName = _settings.FileNameTemplate;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "{index}.log";
            }

            fileName = fileName.Replace("{index}", indexName);

            // Strip any date token for size-based rolling to keep a single logical file name
            fileName = System.Text.RegularExpressions.Regex.Replace(
                fileName,
                "\\{date:(?<fmt>[^}]+)\\}",
                string.Empty);

            if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            {
                fileName += ".log";
            }

            return Path.Combine(_resolvedDir, fileName);
        }

        private void EnsureSizeRoll(string basePath)
        {
            try
            {
                if (!File.Exists(basePath)) return;

                var info = new FileInfo(basePath);
                if (info.Length < _maxBytes) return;

                // Find next available suffix
                int next = 1;
                while (File.Exists($"{basePath}.{next}")) next++;

                File.Move(basePath, $"{basePath}.{next}");
            }
            catch
            {
                // ignore rotation failures to avoid breaking tests
            }
        }

        private void EnsureDirectory()
        {
            try
            {
                Directory.CreateDirectory(_resolvedDir);
            }
            catch
            {
                // ignore
            }
        }

        private void ApplyRetentionCleanup()
        {
            try
            {
                if (_settings.RetentionDays <= 0) return;
                if (!Directory.Exists(_resolvedDir)) return;

                var cutoff = DateTime.UtcNow.Date.AddDays(-_settings.RetentionDays);
                foreach (var file in Directory.EnumerateFiles(_resolvedDir, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        if (info.LastWriteTimeUtc.Date < cutoff)
                        {
                            info.Delete();
                        }
                    }
                    catch { /* ignore */ }
                }
            }
            catch
            {
                // ignore
            }
        }

        public void Shutdown()
        {
            // no persistent handles (using AppendAllText), nothing to dispose.
        }

        private static string ResolveDirectory(string root, string configured)
        {
            if (string.IsNullOrWhiteSpace(configured)) return Path.Combine(root, "logs");

            // Treat paths without root as relative to solution root
            if (!Path.IsPathRooted(configured))
            {
                return Path.Combine(root, configured);
            }
            return configured;
        }

        private static string FindRootPath()
        {
            // Mirror logic from LoggingConfig.FindRootPath but locally
            string currentDir = Directory.GetCurrentDirectory();
            while (!Directory.EnumerateFiles(currentDir, "*.sln").Any())
            {
                var parent = Directory.GetParent(currentDir);
                if (parent == null || parent.FullName == currentDir) // reached the root
                    throw new FileNotFoundException("Solution file not found in any parent directory");
                currentDir = parent.FullName;
            }
            return currentDir;
        }
    }
}
