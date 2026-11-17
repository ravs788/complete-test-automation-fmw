using Core.Composition;

namespace Core.Security.AzureKeyVault;

/// <summary>
/// Registers the <c>azure-keyvault</c> secret provider. Select it in <c>providers.json</c>, e.g.
/// <c>"Secrets": ["environment", "azure-keyvault"]</c> (environment variables are then tried first); it needs
/// <c>KEYVAULT_URI</c>.
/// </summary>
public sealed class AzureKeyVaultPlugin : IFrameworkPlugin
{
    public const string Name = "azure-keyvault";

    public void Register(ProviderRegistry registry) =>
        registry.Add<ISecretProvider>(Name, () =>
            AzureKeyVaultSecretProvider.FromEnvironment()
            ?? throw new InvalidOperationException(
                $"Secrets provider '{Name}' is selected in providers.json but {AzureKeyVaultSecretProvider.VaultUriVariable} is not set."));
}
