using System.Text.Json;

namespace Core.Configuration;
/// <summary>
/// Thread-safe, lazily loaded <see cref="TestSettings"/>:
/// <c>testsettings.json</c> (test output directory) → environment overrides (BROWSER, HEADLESS).
/// </summary>
public static class ConfigurationProvider
{
    public const string FileName = "testsettings.json";

    private static readonly Lazy<TestSettings> _lazySettings = new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    public static TestSettings Current => _lazySettings.Value;

    private static TestSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        var settings = File.Exists(path)
            ? JsonSerializer.Deserialize<TestSettings>(
                  File.ReadAllText(path),
                  new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip })
              ?? new TestSettings()
            : new TestSettings();

        if (Environment.GetEnvironmentVariable("BROWSER") is { Length: > 0 } browser)
        {
            settings = settings with { Browser = browser };
        }

        if (bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var headless))
        {
            settings = settings with { Headless = headless };
        }

        return settings;
    }
}
