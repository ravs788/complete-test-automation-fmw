using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Core.Testing;

namespace Tests.UI.Mobile.Infrastructure;

/// <summary>Runs once when the test assembly is loaded, before any test.</summary>
internal static class TestRunSetup
{
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
        Justification = "A test assembly is the entry point of its own test run; this is the one place that runs before every test.")]
    internal static void Initialize() => AllureResults.ClearPreviousRun();
}
