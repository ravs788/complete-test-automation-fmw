using Core.Composition;

namespace Core.Reporting;

/// <summary>
/// What tests call to structure their report (R39), e.g. <c>await Report.Step("Assert cart page loaded", async () => …)</c>.
/// Goes to the reporter selected in <c>providers.json</c> (<see cref="Framework.Reporter"/>).
/// </summary>
public static class Report
{
    /// <summary>Runs <paramref name="body"/> as a named step.</summary>
    public static void Step(string name, Action body) => Framework.Reporter.Step(name, body);

    /// <summary>Runs <paramref name="body"/> as a named step.</summary>
    public static Task Step(string name, Func<Task> body) => Framework.Reporter.StepAsync(name, body);
}
