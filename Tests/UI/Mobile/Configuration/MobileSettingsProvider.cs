using System.Text.Json;

namespace Tests.UI.Mobile.Configuration;

/// <summary>Thread-safe, lazily loaded <see cref="MobileSettings"/> from <c>mobilesettings.json</c> (test output directory).</summary>
public static class MobileSettingsProvider
{
    public const string FileName = "mobilesettings.json";

    private static readonly Lazy<MobileSettings> _lazySettings = new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    public static MobileSettings Current => _lazySettings.Value;

    private static MobileSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        return File.Exists(path)
            ? JsonSerializer.Deserialize<MobileSettings>(
                  File.ReadAllText(path),
                  new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip })
              ?? new MobileSettings()
            : new MobileSettings();
    }
}
