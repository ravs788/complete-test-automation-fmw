using Core.Composition;

namespace Tests.Unit.Composition;

public sealed class ProvidersSettingsTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("providers-").FullName;

    [Fact]
    public void Load_Should_UseConsoleLogging_And_EnvironmentSecrets_When_ThereIsNoFile()
    {
        var settings = ProvidersConfigProvider.Load(_dir);

        Assert.Equal("console", settings.Logging);
        Assert.Equal("console", settings.EffectiveResultsPublisher);
        Assert.Equal(["environment"], settings.EffectiveSecrets);
    }

    [Fact]
    public void Load_Should_ReadTheFile_FromAParentDirectory()
    {
        File.WriteAllText(Path.Combine(_dir, ProvidersConfigProvider.FileName), """
            {
              // comments are allowed
              "Logging": "elastic",
              "Secrets": [ "environment", "azure-keyvault" ]
            }
            """);
        var child = Directory.CreateDirectory(Path.Combine(_dir, "bin", "Debug")).FullName;

        var settings = ProvidersConfigProvider.Load(child);

        Assert.Equal("elastic", settings.Logging);
        Assert.Equal("elastic", settings.EffectiveResultsPublisher);
        Assert.Equal(["environment", "azure-keyvault"], settings.EffectiveSecrets);
    }

    [Fact]
    public void EffectiveResultsPublisher_Should_PreferItsOwnSetting()
    {
        var settings = new ProvidersSettings { Logging = "elastic", ResultsPublisher = "console" };

        Assert.Equal("console", settings.EffectiveResultsPublisher);
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);
}
