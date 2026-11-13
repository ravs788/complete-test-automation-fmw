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
/// Missing values keep the defaults declared on the record.
/// </summary>
public static class SettingsLoader
{
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

        return builder.Build().Get<T>() ?? new T();
    }
}
