using Core.Security;
using Microsoft.Extensions.Configuration;

namespace Core.Configuration;

/// <summary>
/// Loads a settings record the same way for every settings file in the framework, in this order (later wins):
/// <list type="number">
/// <item>the JSON file (optional; comments allowed),</item>
/// <item>environment variables starting with <c>envPrefix</c>, with <c>__</c> between nested keys
/// (e.g. <c>TESTS_RestfulBooker__Password</c>),</item>
/// <item>short aliases for common settings (e.g. <c>BROWSER</c> → <c>Browser</c>).</item>
/// </list>
/// Missing values keep the defaults declared on the record. Any value of the form <c>secret://NAME</c> is then replaced by
/// <see cref="SecretProvider"/> (R36), so secrets never need to be written into the JSON files.
/// </summary>
public static class SettingsLoader
{
    public const string SecretScheme = "secret://";

    /// <summary>
    /// Resolves <c>secret://NAME</c> placeholders. Defaults to environment variables; replace it (e.g. with a Key Vault
    /// provider) before the first settings load.
    /// </summary>
    public static ISecretProvider SecretProvider { get; set; } = new EnvironmentSecretProvider();

    public static T Load<T>(string directory, string fileName, string envPrefix, IReadOnlyDictionary<string, string>? aliases = null)
        where T : class, new()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile(fileName, optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(envPrefix);

        if (aliases is not null)
        {
            var values = aliases
                .Select(alias => (Key: alias.Value, Value: Environment.GetEnvironmentVariable(alias.Key)))
                .Where(pair => !string.IsNullOrEmpty(pair.Value))
                .Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value));
            builder.AddInMemoryCollection(values);
        }

        var resolved = ResolveSecrets(builder.Build(), SecretProvider);
        foreach (var pair in resolved.AsEnumerable())
        {
            var key = pair.Key.Split(':')[^1];
            if (key.Contains("password", StringComparison.OrdinalIgnoreCase) || key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("token", StringComparison.OrdinalIgnoreCase) || key.Equals("ApiKey", StringComparison.OrdinalIgnoreCase))
            {
                DiagnosticRedactor.RegisterSecret(pair.Value);
            }
        }
        return resolved.Get<T>() ?? new T();
    }

    /// <summary>Replaces every <c>secret://NAME</c> value; fails with the setting's key if a secret can't be found.</summary>
    internal static IConfiguration ResolveSecrets(IConfiguration config, ISecretProvider secrets)
    {
        var placeholders = config.AsEnumerable()
            .Where(kv => kv.Value is not null && kv.Value.StartsWith(SecretScheme, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (placeholders.Count == 0)
        {
            return config;
        }

        var resolved = placeholders.Select(kv =>
        {
            var name = kv.Value![SecretScheme.Length..];
            var value = secrets.GetSecret(name)
                        ?? throw new InvalidOperationException(
                            $"Setting '{kv.Key}' refers to secret '{name}', which {secrets} doesn't provide.");
            // The provider resolved a secret regardless of the setting's name (e.g. a connection string or Name).
            DiagnosticRedactor.RegisterSecret(value);
            return new KeyValuePair<string, string?>(kv.Key, value);
        });

        return new ConfigurationBuilder().AddConfiguration(config).AddInMemoryCollection(resolved).Build();
    }
}
