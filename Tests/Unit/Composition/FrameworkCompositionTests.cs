using Core.Composition;
using Core.Logging;
using Core.Logging.Console;
using Core.Logging.Elastic;
using Core.Security;
using Core.Security.AzureKeyVault;

namespace Tests.Unit.Composition;

/// <summary>
/// Provider selection (R40), tested through the pure helpers so no test changes the run's global configuration.
/// </summary>
public class FrameworkCompositionTests
{
    private sealed class FixedSecrets(string? value) : ISecretProvider
    {
        public string? GetSecret(string name) => value;
    }

    private sealed class SecretsPlugin(string name, string? value) : IFrameworkPlugin
    {
        public void Register(ProviderRegistry registry) => registry.Add<ISecretProvider>(name, () => new FixedSecrets(value));
    }

    private sealed class FakeLoggingFactory(ILoggingService service, bool available) : ILoggingProviderFactory
    {
        public string Name => "fake";

        public bool TryCreate(LoggingConfig cfg, string indexFormat, out ILoggingService created, out string? reason)
        {
            created = service;
            reason = available ? null : "fake backend is down";
            return available;
        }
    }

    [Fact]
    public void Secrets_Should_DefaultTo_EnvironmentVariables()
    {
        var provider = Framework.BuildSecretProvider(Framework.CreateRegistry([]), new ProvidersSettings());

        Assert.IsType<EnvironmentSecretProvider>(provider);
    }

    [Fact]
    public void Secrets_Should_TryTheSelectedProviders_InOrder()
    {
        var registry = Framework.CreateRegistry([new SecretsPlugin("first", null), new SecretsPlugin("second", "from-second")]);

        var provider = Framework.BuildSecretProvider(registry, new ProvidersSettings { Secrets = ["first", "second"] });

        Assert.Equal("from-second", provider.GetSecret("anything"));
    }

    [Fact]
    public void Secrets_Should_FailAtStartup_When_ASelectedProviderIsNotRegistered()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Framework.BuildSecretProvider(Framework.CreateRegistry([]), new ProvidersSettings { Secrets = ["environment", "azure-keyvault"] }));

        Assert.Contains("'azure-keyvault' is not registered", ex.Message);
    }

    [Fact]
    public void KeyVault_Should_ExplainTheMissingVaultUri_When_SelectedWithoutIt()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AzureKeyVaultSecretProvider.VaultUriVariable)))
        {
            return; // a real vault is configured on this machine; nothing to check
        }
        var registry = Framework.CreateRegistry([new AzureKeyVaultPlugin()]);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            Framework.BuildSecretProvider(registry, new ProvidersSettings { Secrets = ["environment", AzureKeyVaultPlugin.Name] }));

        Assert.Contains(AzureKeyVaultSecretProvider.VaultUriVariable, ex.Message);
    }

    [Fact]
    public void Logging_Should_UseTheSelectedProvider()
    {
        var expected = new ConsoleLoggingService();
        var registry = Framework.CreateRegistry([]);
        registry.Add<ILoggingProviderFactory>("fake", () => new FakeLoggingFactory(expected, available: true));

        Assert.Same(expected, LoggingServiceFactory.Create(registry, "fake", new LoggingConfig(), "idx-{0}"));
    }

    [Theory]
    [InlineData("not-registered")]
    [InlineData("fake")]
    public void Logging_Should_FallBackToConsole_When_TheProviderIsMissingOrUnavailable(string provider)
    {
        var unavailable = new ConsoleLoggingService();
        var registry = Framework.CreateRegistry([]);
        registry.Add<ILoggingProviderFactory>("fake", () => new FakeLoggingFactory(unavailable, available: false));

        var service = LoggingServiceFactory.Create(registry, provider, new LoggingConfig(), "idx-{0}");

        Assert.IsType<ConsoleLoggingService>(service);
        Assert.NotSame(unavailable, service);
    }

    [Fact]
    public void ElasticPlugin_Should_Register_Logging_And_ResultsPublishing()
    {
        var registry = Framework.CreateRegistry([new ElasticLoggingPlugin()]);

        Assert.Equal(["console", "elastic"], registry.Names<ILoggingProviderFactory>());
        Assert.Equal(["console", "elastic"], registry.Names<IResultsPublisherFactory>());
    }

    [Fact]
    public void ResultsPublisher_Should_FallBackToConsole_When_TheProviderIsNotRegistered()
    {
        var publisher = ResultsPublisherFactory.Create(Framework.CreateRegistry([]), "elastic", new LoggingConfig());

        Assert.IsType<ConsoleResultsPublisher>(publisher);
    }
}
