using Allure.Net.Commons;

namespace Core.Reporting.Allure;

/// <summary><see cref="ITestReporter"/> on Allure (<c>Allure.Net.Commons</c>); results go to the run's <c>allure-results</c> folder.</summary>
public sealed class AllureTestReporter : ITestReporter
{
    /// <summary>Clears the results of the previous run in this output folder (Allure.Xunit only appends), R23.</summary>
    public void BeginRun() => AllureLifecycle.Instance.CleanupResultDirectory();

    public void UpdateTestName(Func<string, string> update) =>
        TryUpdateTestCase(tc => tc.name = update(tc.name));

    public void AddParameter(string name, string value) =>
        TryUpdateTestCase(tc => tc.parameters.Add(new Parameter { name = name, value = value }));

    public void Attach(string name, string contentType, byte[] content, string fileExtension) =>
        AllureApi.AddAttachment(name, contentType, content, fileExtension);

    public void Step(string name, Action body) => AllureApi.Step(name, body);

    public Task StepAsync(string name, Func<Task> body) => AllureApi.Step(name, body);

    private static void TryUpdateTestCase(Action<TestResult> update)
    {
        try
        {
            AllureLifecycle.Instance.UpdateTestCase(update);
        }
        catch (Exception)
        {
            // No active Allure test case (e.g. code running outside a test); reporting is best effort.
        }
    }
}
