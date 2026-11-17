using Azure;
using Core.Security;
using Core.Security.AzureKeyVault;

namespace Tests.Unit.Security;

public class AzureKeyVaultSecretProviderTests
{
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
