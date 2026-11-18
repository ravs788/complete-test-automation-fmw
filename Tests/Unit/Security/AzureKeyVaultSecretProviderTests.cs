using Azure;
using Core.Security;
using Core.Security.AzureKeyVault;

namespace Tests.Unit.Security;

public class AzureKeyVaultSecretProviderTests
{
    [Fact]
    public void GetSecret_Should_ReturnTheVaultValue_And_CacheIt()
    {
        // Arrange
        var client = new FakeSecretClient(new() { ["booker-password"] = "from-vault" });
        var provider = new AzureKeyVaultSecretProvider(client);

        // Act & Assert
        Assert.Equal("from-vault", provider.GetSecret("booker-password"));
        Assert.Equal("from-vault", provider.GetSecret("booker-password"));

        // Assert: repeated reads used a single vault request.
        Assert.Equal(1, client.Calls);
    }

    [Fact]
    public void GetSecret_Should_ReturnNull_When_TheSecretDoesNotExist()
    {
        // Arrange: use a fake vault with no stored secrets.
        // Act & Assert
        Assert.Null(new AzureKeyVaultSecretProvider(new FakeSecretClient([])).GetSecret("missing"));
    }

    [Fact]
    public void GetSecret_Should_SurfaceAccessErrors()
    {
        // Arrange
        var provider = new AzureKeyVaultSecretProvider(new FakeSecretClient([], failWithStatus: 403));

        // Act & Assert
        Assert.Equal(403, Assert.Throws<RequestFailedException>(() => provider.GetSecret("anything")).Status);
    }

    [Fact]
    public void Composite_Should_PreferTheFirstProvider_And_FallBackToTheVault()
    {
        // Arrange
        var variable = $"UT_KV_{Guid.NewGuid():N}".ToUpperInvariant();
        Environment.SetEnvironmentVariable(variable, "from-env");
        try
        {
            var vault = new AzureKeyVaultSecretProvider(new FakeSecretClient(new() { [variable] = "from-vault", ["only-in-vault"] = "vault-value" }));
            var secrets = new CompositeSecretProvider(new EnvironmentSecretProvider(), vault);

            // Act & Assert: resolve each source and verify precedence, fallback, and missing values.
            Assert.Equal("from-env", secrets.GetSecret(variable));
            Assert.Equal("vault-value", secrets.GetSecret("only-in-vault"));
            Assert.Null(secrets.GetSecret("nowhere"));
        }
        finally
        {
            // Cleanup: restore the environment.
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Fact]
    public void FromEnvironment_Should_ReturnNull_When_NoVaultIsConfigured()
    {
        // Arrange: only exercise the missing-vault case when no vault URI is configured.
        if (Environment.GetEnvironmentVariable(AzureKeyVaultSecretProvider.VaultUriVariable) is null)
        {
            // Act & Assert: creating a provider without a configured vault returns null.
            Assert.Null(AzureKeyVaultSecretProvider.FromEnvironment());
        }
    }
}
