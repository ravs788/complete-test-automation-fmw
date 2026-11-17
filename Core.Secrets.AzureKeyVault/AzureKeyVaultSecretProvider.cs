using System.Collections.Concurrent;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Security;

namespace Core.Secrets.AzureKeyVault;

/// <summary>
/// Resolves <c>secret://NAME</c> placeholders from Azure Key Vault (R36). Each secret is read once per run and cached.
/// A missing secret returns null (so another provider can answer); access problems (no permission, wrong vault)
/// are thrown so they aren't mistaken for a missing secret.
/// </summary>
public sealed class AzureKeyVaultSecretProvider(SecretClient client) : ISecretProvider
{
    /// <summary>Environment variable that turns the provider on: the vault URI, e.g. https://my-vault.vault.azure.net/.</summary>
    public const string VaultUriVariable = "KEYVAULT_URI";

    private readonly ConcurrentDictionary<string, Lazy<string?>> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// A provider for the vault in <c>KEYVAULT_URI</c>, signed in with <see cref="DefaultAzureCredential"/> (az login,
    /// environment credentials, managed identity or a pipeline service connection); null when the variable isn't set.
    /// </summary>
    public static AzureKeyVaultSecretProvider? FromEnvironment() =>
        Environment.GetEnvironmentVariable(VaultUriVariable) is { Length: > 0 } uri
            ? new AzureKeyVaultSecretProvider(new SecretClient(new Uri(uri), new DefaultAzureCredential()))
            : null;

    public string? GetSecret(string name) => _cache.GetOrAdd(name, n => new Lazy<string?>(() => Fetch(n))).Value;

    private string? Fetch(string name)
    {
        try
        {
            // The (name, version, cancellationToken) overload: the latest version of the secret.
            return client.GetSecret(name, null, CancellationToken.None).Value.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public override string ToString() => $"Azure Key Vault {client.VaultUri}";
}
