using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Core.Composition;
using Core.Logging.Elastic;
using Core.Security.AzureKeyVault;
using Core.Testing;

namespace Tests.Infrastructure;

/// <summary>Runs once when the test assembly is loaded, before any test.</summary>
internal static class TestRunSetup
{
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
        Justification = "A test assembly is the entry point of its own test run; this is the one place that runs before every test.")]
    internal static void Initialize()
    {
        AllureResults.ClearPreviousRun();

        // Composition root (R40): the adapters this test project references. Which one is used for each contract is
        // chosen by name in providers.json (e.g. "Logging": "elastic", "Secrets": ["environment", "azure-keyvault"]).
        Framework.Configure(new ElasticLoggingPlugin(), new AzureKeyVaultPlugin());
    }
}
