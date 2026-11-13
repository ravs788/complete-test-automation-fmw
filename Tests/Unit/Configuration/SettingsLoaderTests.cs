using Core.Configuration;

namespace Tests.Unit.Configuration;

/// <summary>
/// Each test uses its own prefix and alias names, so tests that set environment variables can't affect each other.
/// </summary>
public sealed class SettingsLoaderTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("settings-loader-").FullName;
    private readonly string _prefix = $"UT{Guid.NewGuid():N}_";
    private readonly List<string> _envVars = [];

    public sealed record Sample
    {
        public string Name { get; init; } = "default-name";
        public int Count { get; init; } = 1;
        public Section Inner { get; init; } = new();
    }

    public sealed record Section
    {
        public string Url { get; init; } = "default-url";
    }

    [Fact]
    public void Load_Should_KeepDefaults_When_FileAndEnvironmentAreMissing()
    {
        var sample = SettingsLoader.Load<Sample>(_dir, "missing.json", _prefix);

        Assert.Equal(new Sample(), sample);
    }

    [Fact]
    public void Load_Should_ReadJsonWithComments()
    {
        WriteJson("""
                  {
                    // comment
                    "Name": "from-json",
                    "Inner": { "Url": "json-url" }
                  }
                  """);

        var sample = SettingsLoader.Load<Sample>(_dir, "settings.json", _prefix);

        Assert.Equal("from-json", sample.Name);
        Assert.Equal("json-url", sample.Inner.Url);
        Assert.Equal(1, sample.Count);
    }

    [Fact]
    public void Load_Should_PreferPrefixedEnvironmentVariables_Over_Json()
    {
        WriteJson("""{ "Name": "from-json", "Inner": { "Url": "json-url" } }""");
        SetEnv(_prefix + "Name", "from-env");
        SetEnv(_prefix + "Inner__Url", "env-url");

        var sample = SettingsLoader.Load<Sample>(_dir, "settings.json", _prefix);

        Assert.Equal("from-env", sample.Name);
        Assert.Equal("env-url", sample.Inner.Url);
    }

    [Fact]
    public void Load_Should_PreferAliases_Over_PrefixedEnvironmentVariables()
    {
        var alias = $"{_prefix}ALIAS_URL";
        SetEnv(_prefix + "Inner__Url", "env-url");
        SetEnv(alias, "alias-url");

        var sample = SettingsLoader.Load<Sample>(_dir, "settings.json", _prefix, new Dictionary<string, string> { [alias] = "Inner:Url" });

        Assert.Equal("alias-url", sample.Inner.Url);
    }

    [Fact]
    public void Load_Should_IgnoreEmptyAliases()
    {
        WriteJson("""{ "Name": "from-json" }""");
        var alias = $"{_prefix}ALIAS_NAME";
        SetEnv(alias, "");

        var sample = SettingsLoader.Load<Sample>(_dir, "settings.json", _prefix, new Dictionary<string, string> { [alias] = "Name" });

        Assert.Equal("from-json", sample.Name);
    }

    private void WriteJson(string json) => File.WriteAllText(Path.Combine(_dir, "settings.json"), json);

    private void SetEnv(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value);
        _envVars.Add(name);
    }

    public void Dispose()
    {
        foreach (var name in _envVars)
        {
            Environment.SetEnvironmentVariable(name, null);
        }
        Directory.Delete(_dir, recursive: true);
    }
}
