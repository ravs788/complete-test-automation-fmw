using System.Collections.Concurrent;

namespace Core.Composition;

/// <summary>
/// Named providers per contract (R40). Registration is explicit (plugins call <see cref="Add{T}"/>); nothing is
/// discovered by scanning types. Each provider is created on first use and then reused.
/// </summary>
public sealed class ProviderRegistry
{
    private readonly ConcurrentDictionary<(Type Contract, string Name), Lazy<object>> _providers = new();

    /// <summary>Registers <paramref name="create"/> as the provider called <paramref name="name"/> (case-insensitive) for <typeparamref name="T"/>.</summary>
    /// <exception cref="InvalidOperationException">A provider with that name is already registered for <typeparamref name="T"/>.</exception>
    public void Add<T>(string name, Func<T> create) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(create);
        var key = (typeof(T), Normalize(name));
        if (!_providers.TryAdd(key, new Lazy<object>(() => create(), LazyThreadSafetyMode.ExecutionAndPublication)))
        {
            throw new InvalidOperationException($"A {typeof(T).Name} provider named '{name}' is already registered.");
        }
    }

    /// <summary>Returns the provider called <paramref name="name"/> for <typeparamref name="T"/>, creating it on first use.</summary>
    public bool TryGet<T>(string name, out T provider) where T : class
    {
        if (_providers.TryGetValue((typeof(T), Normalize(name)), out var lazy))
        {
            provider = (T)lazy.Value;
            return true;
        }
        provider = null!;
        return false;
    }

    /// <summary>Returns the provider called <paramref name="name"/>, or fails listing the registered names for <paramref name="kind"/>.</summary>
    /// <exception cref="InvalidOperationException">No provider with that name is registered.</exception>
    public T Get<T>(string kind, string name) where T : class =>
        TryGet<T>(name, out var provider)
            ? provider
            : throw new InvalidOperationException(
                $"{kind} provider '{name}' is not registered. Registered: {string.Join(", ", Names<T>())}. " +
                "Check providers.json / PROVIDERS_* and that the test project's TestRunSetup passes the adapter's plugin to Framework.Configure.");

    /// <summary>The registered provider names for <typeparamref name="T"/>, sorted.</summary>
    public IReadOnlyList<string> Names<T>() where T : class =>
        _providers.Keys.Where(k => k.Contract == typeof(T)).Select(k => k.Name).Order(StringComparer.Ordinal).ToList();

    private static string Normalize(string name) => name.Trim().ToLowerInvariant();
}
