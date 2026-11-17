using Core.Composition;
using Core.Reporting;
using Core.Reporting.Allure;

namespace Tests.Unit.Reporting;

/// <summary>Reporting contract (R39): selection, the console reporter and the <see cref="Report"/> helper.</summary>
public class TestReporterTests
{
    [Fact]
    public void ConsoleReporter_Should_RunStepBodies_And_LetFailuresThrough()
    {
        var reporter = new ConsoleTestReporter();
        var ran = false;

        reporter.Step("passes", () => ran = true);
        var ex = Assert.Throws<InvalidOperationException>(() => reporter.Step("fails", () => throw new InvalidOperationException("boom")));

        Assert.True(ran);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task ConsoleReporter_Should_RunAsyncStepBodies_And_LetFailuresThrough()
    {
        var reporter = new ConsoleTestReporter();
        var ran = false;

        await reporter.StepAsync("passes", async () => { await Task.Yield(); ran = true; });
        await Assert.ThrowsAsync<TimeoutException>(() => reporter.StepAsync("fails", () => throw new TimeoutException()));

        Assert.True(ran);
    }

    [Fact]
    public void ConsoleReporter_Should_IgnoreNames_Parameters_And_Attachments()
    {
        var reporter = new ConsoleTestReporter();

        reporter.BeginRun();
        reporter.UpdateTestName(name => name + " (x)");
        reporter.AddParameter("browser", "chrome");
        reporter.Attach("Test Failure", "image/png", [1, 2, 3], "png");
    }

    [Fact]
    public async Task ReportHelper_Should_RunTheStep_WithTheSelectedReporter()
    {
        var steps = new List<string>();

        Report.Step("sync", () => steps.Add("sync"));
        await Report.Step("async", async () => { await Task.Yield(); steps.Add("async"); });

        Assert.Equal(["sync", "async"], steps);
    }

    [Fact]
    public void Reporter_Should_BeTheSelectedOne_When_Registered()
    {
        var registry = Framework.CreateRegistry([new AllureReportingPlugin()]);

        Assert.IsType<AllureTestReporter>(Framework.ResolveReporter(registry, "allure"));
        Assert.IsType<ConsoleTestReporter>(Framework.ResolveReporter(registry, "console"));
    }

    [Fact]
    public void Reporter_Should_FallBackToConsole_When_NotRegistered()
    {
        Assert.IsType<ConsoleTestReporter>(Framework.ResolveReporter(Framework.CreateRegistry([]), "allure"));
    }

    [Fact]
    public void Reporting_Should_DefaultTo_Console()
    {
        Assert.Equal("console", new ProvidersSettings().Reporting);
    }
}
