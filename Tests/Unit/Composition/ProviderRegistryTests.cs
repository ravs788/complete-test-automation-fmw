using Core.Composition;
using Core.Security;

namespace Tests.Unit.Composition;

public class ProviderRegistryTests
{
    private sealed class FixedSecrets(string value) : ISecretProvider
    {
        public string? GetSecret(string name) => value;
    }

    [Fact]
    public void Get_Should_ReturnTheProvider_RegardlessOfNameCase()
    {
        var registry = new ProviderRegistry();
        registry.Add<ISecretProvider>("Vault-A", () => new FixedSecrets("a"));

        Assert.Equal("a", registry.Get<ISecretProvider>(ProviderKinds.Secrets, " vault-a ").GetSecret("x"));
    }

    [Fact]
    public void Get_Should_CreateEachProvider_OnlyOnce()
    {
        var registry = new ProviderRegistry();
        var created = 0;
        registry.Add<ISecretProvider>("counted", () => { created++; return new FixedSecrets("c"); });

        var first = registry.Get<ISecretProvider>(ProviderKinds.Secrets, "counted");
        var second = registry.Get<ISecretProvider>(ProviderKinds.Secrets, "counted");

        Assert.Same(first, second);
        Assert.Equal(1, created);
    }

    [Fact]
    public void Add_Should_Fail_When_TheNameIsAlreadyRegistered_ForThatContract()
    {
        var registry = new ProviderRegistry();
        registry.Add<ISecretProvider>("dup", () => new FixedSecrets("1"));

        var ex = Assert.Throws<InvalidOperationException>(() => registry.Add<ISecretProvider>("DUP", () => new FixedSecrets("2")));
        Assert.Contains("'DUP'", ex.Message);
    }

    [Fact]
    public void Get_Should_ListTheRegisteredNames_When_TheProviderIsUnknown()
    {
        var registry = Framework.CreateRegistry([]);

        var ex = Assert.Throws<InvalidOperationException>(() => registry.Get<ISecretProvider>(ProviderKinds.Secrets, "vault-x"));

        Assert.Contains("Secrets provider 'vault-x' is not registered", ex.Message);
        Assert.Contains("Registered: environment", ex.Message);
    }

    [Fact]
    public void BuiltIns_Should_Register_TheProvidersThatNeedNoVendorPackage()
    {
        var registry = Framework.CreateRegistry([]);

        Assert.Equal(["console"], registry.Names<Core.Logging.ILoggingProviderFactory>());
        Assert.Equal(["console"], registry.Names<Core.Logging.IResultsPublisherFactory>());
        Assert.Equal(["environment"], registry.Names<ISecretProvider>());
        Assert.Equal(["console"], registry.Names<Core.Reporting.ITestReporter>());
        Assert.Equal(["httpclient"], registry.Names<Core.Http.IHttpTransportFactory>());
        Assert.Equal(["builtin"], registry.Names<Core.Resilience.IRetryPolicyFactory>());
    }
}
