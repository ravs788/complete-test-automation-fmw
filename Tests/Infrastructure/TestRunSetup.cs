using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Core.Composition;
using Core.Http.Playwright;
using Core.Logging.Elastic;
using Core.Reporting.Allure;
using Core.Security.AzureKeyVault;

namespace Tests.Infrastructure;

/// <summary>Runs once when the test assembly is loaded, before any test.</summary>
internal static class TestRunSetup
{
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
        Justification = "A test assembly is the entry point of its own test run; this is the one place that runs before every test.")]
    internal static void Initialize()
    {
        // Composition root (R40): the adapters this test project references. Which one is used for each contract is
        // chosen by name in providers.json (e.g. "Logging": "elastic", "Reporting": "allure"). The selected reporter clears the
        // previous run's results here (R23).
        Framework.Configure(new ElasticLoggingPlugin(), new AllureReportingPlugin(), new PlaywrightHttpPlugin(), new AzureKeyVaultPlugin());
    }
}
