using Core.Logging;
using Core.Logging.Console;
using Core.Security;

namespace Core.Composition;

/// <summary>The providers that ship with Core and need no vendor package: console logging and results, environment secrets.</summary>
public sealed class BuiltInPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry)
    {
        registry.Add<ILoggingProviderFactory>("console", () => new ConsoleLoggingProviderFactory());
        registry.Add<IResultsPublisherFactory>("console", () => new ConsoleResultsPublisherFactory());
        registry.Add<ISecretProvider>("environment", () => new EnvironmentSecretProvider());
    }
}
