namespace Core.Configuration;

/// <summary>Finds repo-level settings files (e.g. <c>logging-config.json</c>, <c>providers.json</c>) by walking up from a start directory.</summary>
public static class ConfigFileLocator
{
    /// <summary>
    /// Returns the nearest directory, starting at <paramref name="startDirectory"/> (default: the current directory)
    /// and walking up, that contains <paramref name="fileName"/>; null if none does.
    /// </summary>
    public static string? FindDirectoryContaining(string fileName, string? startDirectory = null)
    {
        var dir = new DirectoryInfo(startDirectory ?? Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, fileName)))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }
}
