using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Core.Configuration;
using Core.Secrets.AzureKeyVault;
using Core.Security;
using Core.Testing;

namespace Tests.UI.Mobile.Infrastructure;

/// <summary>Runs once when the test assembly is loaded, before any test.</summary>
internal static class TestRunSetup
{
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
        Justification = "A test assembly is the entry point of its own test run; this is the one place that runs before every test.")]
    internal static void Initialize()
    {
        AllureResults.ClearPreviousRun();

        // R36: with KEYVAULT_URI set, secret://NAME placeholders also resolve from Azure Key Vault
        // (environment variables still win, so local overrides keep working).
        if (AzureKeyVaultSecretProvider.FromEnvironment() is { } vault)
        {
            SettingsLoader.SecretProvider = new CompositeSecretProvider(new EnvironmentSecretProvider(), vault);
        }
    }
}
