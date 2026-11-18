using Allure.Net.Commons;
using Core.Security;

namespace Core.Reporting.Allure;

/// <summary><see cref="ITestReporter"/> on Allure (<c>Allure.Net.Commons</c>); results go to the run's <c>allure-results</c> folder.</summary>
public sealed class AllureTestReporter : ITestReporter
{
    /// <summary>Clears the results of the previous run in this output folder (Allure.Xunit only appends), R23.</summary>
    public void BeginRun() => AllureLifecycle.Instance.CleanupResultDirectory();

    public void UpdateTestName(Func<string, string> update) =>
        TryUpdateTestCase(tc => tc.name = update(tc.name));

    /// <summary>
    /// Replaces the suite labels Allure.Xunit derived from the class with the framework's and adds the tags, so test
    /// classes need no Allure attributes (R48).
    /// </summary>
    public void SetSuite(string parentSuite, string suite, string subSuite, IReadOnlyList<string> tags) =>
        TryUpdateTestCase(tc =>
        {
            tc.labels.RemoveAll(l => l.name is "parentSuite" or "suite" or "subSuite");
            tc.labels.Add(Label.ParentSuite(parentSuite));
            tc.labels.Add(Label.Suite(suite));
            tc.labels.Add(Label.SubSuite(subSuite));
            foreach (var tag in tags)
            {
                tc.labels.Add(Label.Tag(tag));
            }
        });

    public void AddParameter(string name, string value) =>
        TryUpdateTestCase(tc => tc.parameters.Add(new Parameter { name = name, value = DiagnosticRedactor.Redact(value) }));

    public void Attach(string name, string contentType, byte[] content, string fileExtension)
    {
        if (DiagnosticArtifactPolicy.Current.AllowAttachments)
        {
            AllureApi.AddAttachment(DiagnosticRedactor.Redact(name), contentType, content, fileExtension);
        }
    }

    public void Step(string name, Action body) => AllureApi.Step(DiagnosticRedactor.Redact(name), () => RedactedTestException.Run(body));

    public Task StepAsync(string name, Func<Task> body) => AllureApi.Step(DiagnosticRedactor.Redact(name), () => RedactedTestException.RunAsync(body));

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
