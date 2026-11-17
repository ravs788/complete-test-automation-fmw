using Core.Configuration;
using Core.Http;
using Core.Logging;
using Core.Reporting;
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
    private static ITestReporter? _reporter;

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

    /// <summary>The test reporter selected in <c>providers.json</c> (console if it isn't registered, or before <c>Configure</c>).</summary>
    public static ITestReporter Reporter
    {
        get
        {
            lock (Gate)
            {
                return _reporter ??= ResolveReporter(_registry ??= CreateRegistry([]), _settings.Reporting);
            }
        }
    }

    /// <summary>The HTTP transport factory selected in <c>providers.json</c>; fails listing the registered names if it isn't registered.</summary>
    public static IHttpTransportFactory HttpTransports => Providers.Get<IHttpTransportFactory>(ProviderKinds.Http, Settings.Http);

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
        var reporter = ResolveReporter(registry, settings.Reporting);
        lock (Gate)
        {
            _registry = registry;
            _settings = settings;
            _reporter = reporter;
            SettingsLoader.SecretProvider = secrets;
        }
        reporter.BeginRun();
        System.Console.WriteLine(
            $"[Providers] {ProviderKinds.Logging}={settings.Logging}, {ProviderKinds.ResultsPublisher}={settings.EffectiveResultsPublisher}, " +
            $"{ProviderKinds.Secrets}={string.Join(" > ", settings.EffectiveSecrets)}, {ProviderKinds.Reporting}={settings.Reporting}, {ProviderKinds.Http}={settings.Http}");
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

    /// <summary>The reporter called <paramref name="name"/>, or console (with one message) if it isn't registered.</summary>
    public static ITestReporter ResolveReporter(ProviderRegistry registry, string name)
    {
        if (registry.TryGet<ITestReporter>(name, out var reporter))
        {
            return reporter;
        }
        ProviderFallback.Report(name, "not registered", "console", ProviderKinds.Reporting);
        return registry.TryGet<ITestReporter>("console", out var console) ? console : new ConsoleTestReporter();
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
