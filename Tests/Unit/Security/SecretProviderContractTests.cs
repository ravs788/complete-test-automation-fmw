using Core.Security;
using Core.Security.AzureKeyVault;

namespace Tests.Unit.Security;

/// <summary>The behaviour every <see cref="ISecretProvider"/> must have (R46): known secret → value, unknown → null.</summary>
public abstract class SecretProviderContractTests
{
    /// <summary>A provider that knows exactly one secret, <paramref name="name"/> = <paramref name="value"/>.</summary>
    protected abstract ISecretProvider CreateWithOneSecret(string name, string value);

    [Fact]
    public void KnownSecret_Should_ReturnItsValue()
    {
        // Arrange
        var name = $"contract-secret-{Guid.NewGuid():N}";

        // Act & Assert
        Assert.Equal("s3cret", CreateWithOneSecret(name, "s3cret").GetSecret(name));
    }

    [Fact]
    public void UnknownSecret_Should_ReturnNull()
    {
        // Arrange
        var provider = CreateWithOneSecret($"contract-secret-{Guid.NewGuid():N}", "x");

        // Act & Assert
        Assert.Null(provider.GetSecret($"missing-{Guid.NewGuid():N}"));
    }
}

public sealed class EnvironmentSecretProviderContractTests : SecretProviderContractTests
{
    protected override ISecretProvider CreateWithOneSecret(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value);
        return new EnvironmentSecretProvider();
    }
}

public sealed class AzureKeyVaultSecretProviderContractTests : SecretProviderContractTests
{
    protected override ISecretProvider CreateWithOneSecret(string name, string value) =>
        new AzureKeyVaultSecretProvider(new FakeSecretClient(new() { [name] = value }));
}
