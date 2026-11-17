namespace Core.Reporting;

/// <summary>
/// Test reporting contract (R39): what the framework and tests need from a report, independent of the reporting
/// library. Implementations must never fail a test: everything except a step's own body is best effort.
/// Selected with <c>"Reporting"</c> in <c>providers.json</c>; tests use the <see cref="Report"/> helper.
/// </summary>
public interface ITestReporter
{
    /// <summary>Called once per run before any test, e.g. to clear the previous run's results.</summary>
    void BeginRun();

    /// <summary>Renames the current test, e.g. <c>name => $"{name} (chrome)"</c>.</summary>
    void UpdateTestName(Func<string, string> update);

    /// <summary>Adds a name/value parameter to the current test.</summary>
    void AddParameter(string name, string value);

    /// <summary>Attaches a file (screenshot, trace, …) to the current test.</summary>
    void Attach(string name, string contentType, byte[] content, string fileExtension);

    /// <summary>Runs <paramref name="body"/> as a named step; its exceptions propagate unchanged.</summary>
    void Step(string name, Action body);

    /// <summary>Runs <paramref name="body"/> as a named step; its exceptions propagate unchanged.</summary>
    Task StepAsync(string name, Func<Task> body);
}
