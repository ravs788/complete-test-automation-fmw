using Core.Configuration;
using Core.Security;
using Microsoft.Extensions.Configuration;

namespace Tests.Unit.Configuration;

public class SecretResolutionTests
{
    private sealed class FakeSecrets(Dictionary<string, string> values) : ISecretProvider
    {
        public string? GetSecret(string name) => values.GetValueOrDefault(name);
        public override string ToString() => "the fake provider";
    }

    private static IConfiguration Config(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void ResolveSecrets_Should_ReplacePlaceholders_And_LeaveOtherValues()
    {
        var config = Config(new() { ["Elastic:Password"] = "secret://elastic-password", ["Elastic:Url"] = "http://localhost:9200" });

        var resolved = SettingsLoader.ResolveSecrets(config, new FakeSecrets(new() { ["elastic-password"] = "s3cret" }));

        Assert.Equal("s3cret", resolved["Elastic:Password"]);
        Assert.Equal("http://localhost:9200", resolved["Elastic:Url"]);
    }

    [Fact]
    public void ResolveSecrets_Should_RegisterOpaqueValuesUnderOrdinarySettingNames()
    {
        var secret = $"opaque-{Guid.NewGuid():N}/credential";
        var config = Config(new() { ["Name"] = "secret://opaque" });

        var resolved = SettingsLoader.ResolveSecrets(config, new FakeSecrets(new() { ["opaque"] = secret }));

        Assert.Equal(secret, resolved["Name"]);
        Assert.DoesNotContain(secret, DiagnosticRedactor.Redact($"operation failed with {secret}"));
        Assert.DoesNotContain(Uri.EscapeDataString(secret), DiagnosticRedactor.Redact($"encoded {Uri.EscapeDataString(secret)}"));
    }

    [Fact]
    public void ResolveSecrets_Should_NameTheSetting_When_ASecretIsMissing()
    {
        var config = Config(new() { ["RestfulBooker:Password"] = "secret://booker-password" });

        var ex = Assert.Throws<InvalidOperationException>(() => SettingsLoader.ResolveSecrets(config, new FakeSecrets([])));

        Assert.Contains("RestfulBooker:Password", ex.Message);
        Assert.Contains("booker-password", ex.Message);
    }

    [Fact]
    public void EnvironmentSecretProvider_Should_FindTheUpperCaseUnderscoreName()
    {
        var name = $"ut-secret-{Guid.NewGuid():N}";
        var variable = name.ToUpperInvariant().Replace('-', '_');
        Environment.SetEnvironmentVariable(variable, "from-env");
        try
        {
            Assert.Equal("from-env", new EnvironmentSecretProvider().GetSecret(name));
            Assert.Null(new EnvironmentSecretProvider().GetSecret(name + "-missing"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Fact]
    public void Load_Should_ResolvePlaceholdersFromTheJsonFile()
    {
        var dir = Directory.CreateTempSubdirectory("secrets-").FullName;
        var variable = $"UT_SECRET_{Guid.NewGuid():N}".ToUpperInvariant();
        File.WriteAllText(Path.Combine(dir, "s.json"), $$"""{ "Name": "secret://{{variable}}" }""");
        Environment.SetEnvironmentVariable(variable, "resolved");
        try
        {
            Assert.Equal("resolved", SettingsLoader.Load<SettingsLoaderTests.Sample>(dir, "s.json", $"UT{Guid.NewGuid():N}_").Name);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
            Directory.Delete(dir, recursive: true);
        }
    }
}
