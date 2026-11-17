namespace Core.Security;

/// <summary>
/// Resolves secrets from environment variables (or CI secret variables). <c>secret://elastic-password</c> is looked up
/// as <c>elastic-password</c>, then as <c>ELASTIC_PASSWORD</c>.
/// </summary>
public sealed class EnvironmentSecretProvider : ISecretProvider
{
    public string? GetSecret(string name) =>
        Read(name) ?? Read(name.ToUpperInvariant().Replace('-', '_').Replace('.', '_'));

    private static string? Read(string variable) =>
        Environment.GetEnvironmentVariable(variable) is { Length: > 0 } value ? value : null;

    public override string ToString() => "environment variables";
}
