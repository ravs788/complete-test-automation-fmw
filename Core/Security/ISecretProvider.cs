namespace Core.Security;

/// <summary>
/// Supplies secret values for <c>secret://NAME</c> placeholders in settings files (R36), so passwords and tokens never
/// sit in committed JSON. The default reads environment variables (<see cref="EnvironmentSecretProvider"/>); a vault
/// (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) can be plugged in by implementing this interface and setting
/// <see cref="Configuration.SettingsLoader.SecretProvider"/> before the first settings load.
/// </summary>
public interface ISecretProvider
{
    /// <summary>The secret called <paramref name="name"/>, or null when this provider doesn't have it.</summary>
    string? GetSecret(string name);
}
