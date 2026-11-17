using Core.Configuration;

namespace Core.Composition;

/// <summary>
/// Loads <see cref="ProvidersSettings"/> once per run from <c>providers.json</c> (found by walking up from the run
/// directory, like <c>logging-config.json</c>) plus <c>PROVIDERS_*</c> environment variables. Without the file, the
/// defaults apply (console logging, environment secrets).
/// </summary>
public static class ProvidersConfigProvider
{
    public const string FileName = "providers.json";
    public const string EnvPrefix = "PROVIDERS_";

    private static readonly Lazy<ProvidersSettings> _lazy = new(() => Load(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ProvidersSettings Current => _lazy.Value;

    /// <summary>Loads the settings from <paramref name="startDirectory"/> (default: the current directory) upwards.</summary>
    public static ProvidersSettings Load(string? startDirectory = null)
    {
        var dir = ConfigFileLocator.FindDirectoryContaining(FileName, startDirectory)
                  ?? startDirectory ?? Directory.GetCurrentDirectory();
        return SettingsLoader.Load<ProvidersSettings>(dir, FileName, EnvPrefix);
    }
}
