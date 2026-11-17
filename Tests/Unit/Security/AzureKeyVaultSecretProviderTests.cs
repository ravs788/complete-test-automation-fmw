using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Core.Security;
using Core.Security.AzureKeyVault;

namespace Tests.Unit.Security;

public class AzureKeyVaultSecretProviderTests
{
    /// <summary>A Key Vault client double: SecretClient is designed to be mocked through its protected constructor.</summary>
    private sealed class FakeSecretClient(Dictionary<string, string> secrets, int failWithStatus = 0) : SecretClient
    {
        public int Calls { get; private set; }
        public override Uri VaultUri => new("https://unit-test.vault.azure.net/");

        public override Response<KeyVaultSecret> GetSecret(string name, string? version = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            if (failWithStatus != 0) throw new RequestFailedException(failWithStatus, "access denied");
            return secrets.TryGetValue(name, out var value)
                ? Response.FromValue(SecretModelFactory.KeyVaultSecret(new SecretProperties(name), value), new FakeResponse())
                : throw new RequestFailedException(404, "SecretNotFound");
        }
    }

    private sealed class FakeResponse : Response
    {
        public override int Status => 200;
        public override string ReasonPhrase => "OK";
        public override Stream? ContentStream { get; set; }
        public override string ClientRequestId { get; set; } = "";
        public override void Dispose() { }
        protected override bool ContainsHeader(string name) => false;
        protected override IEnumerable<HttpHeader> EnumerateHeaders() => [];
        protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value) { value = null; return false; }
        protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values) { values = null; return false; }
    }

    [Fact]
    public void GetSecret_Should_ReturnTheVaultValue_And_CacheIt()
    {
        var client = new FakeSecretClient(new() { ["booker-password"] = "from-vault" });
        var provider = new AzureKeyVaultSecretProvider(client);

        Assert.Equal("from-vault", provider.GetSecret("booker-password"));
        Assert.Equal("from-vault", provider.GetSecret("booker-password"));
        Assert.Equal(1, client.Calls);
    }

    [Fact]
    public void GetSecret_Should_ReturnNull_When_TheSecretDoesNotExist()
    {
        Assert.Null(new AzureKeyVaultSecretProvider(new FakeSecretClient([])).GetSecret("missing"));
    }

    [Fact]
    public void GetSecret_Should_SurfaceAccessErrors()
    {
        var provider = new AzureKeyVaultSecretProvider(new FakeSecretClient([], failWithStatus: 403));

        Assert.Equal(403, Assert.Throws<RequestFailedException>(() => provider.GetSecret("anything")).Status);
    }

    [Fact]
    public void Composite_Should_PreferTheFirstProvider_And_FallBackToTheVault()
    {
        var variable = $"UT_KV_{Guid.NewGuid():N}".ToUpperInvariant();
        Environment.SetEnvironmentVariable(variable, "from-env");
        try
        {
            var vault = new AzureKeyVaultSecretProvider(new FakeSecretClient(new() { [variable] = "from-vault", ["only-in-vault"] = "vault-value" }));
            var secrets = new CompositeSecretProvider(new EnvironmentSecretProvider(), vault);

            Assert.Equal("from-env", secrets.GetSecret(variable));
            Assert.Equal("vault-value", secrets.GetSecret("only-in-vault"));
            Assert.Null(secrets.GetSecret("nowhere"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Fact]
    public void FromEnvironment_Should_ReturnNull_When_NoVaultIsConfigured()
    {
        if (Environment.GetEnvironmentVariable(AzureKeyVaultSecretProvider.VaultUriVariable) is null)
        {
            Assert.Null(AzureKeyVaultSecretProvider.FromEnvironment());
        }
    }
}
