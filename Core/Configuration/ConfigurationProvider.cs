namespace Core.Configuration;

/// <summary>
/// Thread-safe, lazily loaded <see cref="TestSettings"/>: <c>testsettings.json</c> (test output directory), then
/// <c>TESTS_*</c> environment variables (e.g. <c>TESTS_RestfulBooker__Password</c>), then the aliases
/// <c>BROWSER</c> and <c>HEADLESS</c>. See <see cref="SettingsLoader"/>.
/// </summary>
public static class ConfigurationProvider
{
    public const string FileName = "testsettings.json";
    public const string EnvPrefix = "TESTS_";

    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["BROWSER"] = "Browser",
        ["HEADLESS"] = "Headless",
    };

    private static readonly Lazy<TestSettings> _lazySettings = new(
        () => SettingsLoader.Load<TestSettings>(AppContext.BaseDirectory, FileName, EnvPrefix, Aliases),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static TestSettings Current => _lazySettings.Value;
}
