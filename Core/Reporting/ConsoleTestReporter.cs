namespace Core.Reporting;

/// <summary>
/// Built-in reporter (R39) for runs without a reporting library: steps are printed, names and parameters are
/// already in the log lines, and attachments are noted but not kept.
/// </summary>
public sealed class ConsoleTestReporter : ITestReporter
{
    public void BeginRun()
    {
    }

    public void UpdateTestName(Func<string, string> update)
    {
    }

    public void SetSuite(string parentSuite, string suite, string subSuite, IReadOnlyList<string> tags)
    {
    }

    public void AddParameter(string name, string value)
    {
    }

    public void Attach(string name, string contentType, byte[] content, string fileExtension) =>
        System.Console.WriteLine($"[Report] '{name}' ({contentType}, {content.Length} bytes) not kept: console reporter");

    public void Step(string name, Action body)
    {
        System.Console.WriteLine($"[Step] {name}");
        body();
    }

    public async Task StepAsync(string name, Func<Task> body)
    {
        System.Console.WriteLine($"[Step] {name}");
        await body();
    }
}
