using Core.Configuration;
using Core.Security;

namespace Core.Composition;

/// <summary>
/// The composition root (R40). A test project calls <see cref="Configure(IFrameworkPlugin[])"/> once from its
/// <c>TestRunSetup</c> module initializer with the plugins of the adapters it references; the provider for each
/// contract is then chosen by name from <c>providers.json</c>. Code that runs without <c>Configure</c> (e.g. unit
/// tests) gets the built-in providers only.
/// </summary>
public static class Framework
{
    private static readonly object Gate = new();
    private static ProviderRegistry? _registry;
    private static ProvidersSettings _settings = new();

    /// <summary>The registered providers (built-ins only if <see cref="Configure(IFrameworkPlugin[])"/> wasn't called).</summary>
    public static ProviderRegistry Providers
    {
        get
        {
            lock (Gate)
            {
                return _registry ??= CreateRegistry([]);
            }
        }
    }

    /// <summary>The provider selection in use.</summary>
    public static ProvidersSettings Settings
    {
        get
        {
            lock (Gate)
            {
                return _settings;
            }
        }
    }

    /// <summary>Registers the built-in providers plus <paramref name="plugins"/>, using the selection from <c>providers.json</c>.</summary>
    public static ProviderRegistry Configure(params IFrameworkPlugin[] plugins) =>
        Configure(ProvidersConfigProvider.Current, plugins);

    /// <summary>
    /// Registers the built-in providers plus <paramref name="plugins"/>, applies <paramref name="settings"/> (the secret
    /// providers are wired into <see cref="SettingsLoader.SecretProvider"/> straight away), and prints the selection once.
    /// </summary>
    /// <exception cref="InvalidOperationException">A selected secret provider isn't registered or can't be created.</exception>
    public static ProviderRegistry Configure(ProvidersSettings settings, params IFrameworkPlugin[] plugins)
    {
        var registry = CreateRegistry(plugins);
        var secrets = BuildSecretProvider(registry, settings);
        lock (Gate)
        {
            _registry = registry;
            _settings = settings;
            SettingsLoader.SecretProvider = secrets;
        }
        System.Console.WriteLine(
            $"[Providers] {ProviderKinds.Logging}={settings.Logging}, {ProviderKinds.ResultsPublisher}={settings.EffectiveResultsPublisher}, " +
            $"{ProviderKinds.Secrets}={string.Join(" > ", settings.EffectiveSecrets)}");
        return registry;
    }

    /// <summary>The built-in providers plus those of <paramref name="plugins"/>.</summary>
    public static ProviderRegistry CreateRegistry(IEnumerable<IFrameworkPlugin> plugins)
    {
        var registry = new ProviderRegistry();
        new BuiltInPlugin().Register(registry);
        foreach (var plugin in plugins)
        {
            plugin.Register(registry);
        }
        return registry;
    }

    /// <summary>The selected secret providers, tried in order (environment first by default).</summary>
    /// <exception cref="InvalidOperationException">A selected provider isn't registered or can't be created.</exception>
    public static ISecretProvider BuildSecretProvider(ProviderRegistry registry, ProvidersSettings settings)
    {
        var providers = settings.EffectiveSecrets
            .Select(name => registry.Get<ISecretProvider>(ProviderKinds.Secrets, name))
            .ToArray();
        return providers.Length == 1 ? providers[0] : new CompositeSecretProvider(providers);
    }
}
