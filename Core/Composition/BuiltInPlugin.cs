using Core.Http;
using Core.Logging;
using Core.Logging.Console;
using Core.Reporting;
using Core.Security;

namespace Core.Composition;

/// <summary>The providers that ship with Core and need no vendor package: console logging, results and reporting, environment secrets, the HttpClient transport.</summary>
public sealed class BuiltInPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry)
    {
        registry.Add<ILoggingProviderFactory>("console", () => new ConsoleLoggingProviderFactory());
        registry.Add<IResultsPublisherFactory>("console", () => new ConsoleResultsPublisherFactory());
        registry.Add<ISecretProvider>("environment", () => new EnvironmentSecretProvider());
        registry.Add<ITestReporter>("console", () => new ConsoleTestReporter());
        registry.Add<IHttpTransportFactory>("httpclient", () => new HttpClientTransportFactory());
    }
}
