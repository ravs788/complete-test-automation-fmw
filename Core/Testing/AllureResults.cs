using Allure.Net.Commons;

namespace Core.Testing;

/// <summary>Housekeeping for the Allure results folder (<c>bin/&lt;Configuration&gt;/net10.0/allure-results</c>).</summary>
public static class AllureResults
{
    /// <summary>
    /// Deletes the previous run's results. Allure.Xunit never clears the folder, so without this every local
    /// <c>dotnet test</c> adds to the last one and <c>allure serve</c> mixes runs. Called once per test run from
    /// each test project's module initializer. Anything that runs several <c>dotnet test</c> commands and wants
    /// all their results (the scripts, the nightly pipeline) copies the folder out after each run.
    /// </summary>
    public static void ClearPreviousRun() => AllureLifecycle.Instance.CleanupResultDirectory();
}
