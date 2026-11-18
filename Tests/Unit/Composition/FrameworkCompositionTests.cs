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
        // Arrange: use the built-in registry and default provider selection.
        // Act
        var provider = Framework.BuildSecretProvider(Framework.CreateRegistry([]), new ProvidersSettings());

        // Assert
        Assert.IsType<EnvironmentSecretProvider>(provider);
    }

    [Fact]
    public void Secrets_Should_TryTheSelectedProviders_InOrder()
    {
        // Arrange
        var registry = Framework.CreateRegistry([new SecretsPlugin("first", null), new SecretsPlugin("second", "from-second")]);

        // Act
        var provider = Framework.BuildSecretProvider(registry, new ProvidersSettings { Secrets = ["first", "second"] });

        // Assert
        Assert.Equal("from-second", provider.GetSecret("anything"));
    }

    [Fact]
    public void Secrets_Should_FailAtStartup_When_ASelectedProviderIsNotRegistered()
    {
        // Arrange: select a vault provider without registering its plugin.
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Framework.BuildSecretProvider(Framework.CreateRegistry([]), new ProvidersSettings { Secrets = ["environment", "azure-keyvault"] }));

        // Assert: startup identifies the missing provider.
        Assert.Contains("'azure-keyvault' is not registered", ex.Message);
    }

    [Fact]
    public void KeyVault_Should_ExplainTheMissingVaultUri_When_SelectedWithoutIt()
    {
        // Arrange
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AzureKeyVaultSecretProvider.VaultUriVariable)))
        {
            return; // a real vault is configured on this machine; nothing to check
        }
        var registry = Framework.CreateRegistry([new AzureKeyVaultPlugin()]);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Framework.BuildSecretProvider(registry, new ProvidersSettings { Secrets = ["environment", AzureKeyVaultPlugin.Name] }));

        // Assert: the error identifies the missing vault URI setting.
        Assert.Contains(AzureKeyVaultSecretProvider.VaultUriVariable, ex.Message);
    }

    [Fact]
    public void Logging_Should_UseTheSelectedProvider()
    {
        // Arrange
        var expected = new ConsoleLoggingService();
        var registry = Framework.CreateRegistry([]);
        registry.Add<ILoggingProviderFactory>("fake", () => new FakeLoggingFactory(expected, available: true));

        // Act & Assert
        Assert.Same(expected, LoggingServiceFactory.Create(registry, "fake", new LoggingConfig(), "idx-{0}"));
    }

    [Theory]
    [InlineData("not-registered")]
    [InlineData("fake")]
    public void Logging_Should_FallBackToConsole_When_TheProviderIsMissingOrUnavailable(string provider)
    {
        // Arrange
        var unavailable = new ConsoleLoggingService();
        var registry = Framework.CreateRegistry([]);
        registry.Add<ILoggingProviderFactory>("fake", () => new FakeLoggingFactory(unavailable, available: false));

        // Act
        var service = LoggingServiceFactory.Create(registry, provider, new LoggingConfig(), "idx-{0}");

        // Assert
        Assert.IsType<ConsoleLoggingService>(service);
        Assert.NotSame(unavailable, service);
    }

    [Fact]
    public void ElasticPlugin_Should_Register_Logging_And_ResultsPublishing()
    {
        // Arrange: use the Elastic logging plugin.
        // Act
        var registry = Framework.CreateRegistry([new ElasticLoggingPlugin()]);

        // Assert
        Assert.Equal(["console", "elastic"], registry.Names<ILoggingProviderFactory>());
        Assert.Equal(["console", "elastic"], registry.Names<IResultsPublisherFactory>());
    }

    [Fact]
    public void ResultsPublisher_Should_FallBackToConsole_When_TheProviderIsNotRegistered()
    {
        // Arrange: select Elastic publishing without registering its plugin.
        // Act
        var publisher = ResultsPublisherFactory.Create(Framework.CreateRegistry([]), "elastic", new LoggingConfig());

        // Assert
        Assert.IsType<ConsoleResultsPublisher>(publisher);
    }
}
