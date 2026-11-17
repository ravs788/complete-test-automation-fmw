namespace Core.Security;

/// <summary>Asks each provider in turn and returns the first secret found (e.g. environment first, then a vault).</summary>
public sealed class CompositeSecretProvider(params ISecretProvider[] providers) : ISecretProvider
{
    public string? GetSecret(string name) =>
        providers.Select(p => p.GetSecret(name)).FirstOrDefault(value => value is not null);

    public override string ToString() => string.Join(" or ", providers.Select(p => p.ToString()));
}
