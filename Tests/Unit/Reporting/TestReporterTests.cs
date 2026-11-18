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
        // Arrange
        var reporter = new ConsoleTestReporter();
        var ran = false;

        // Act
        reporter.Step("passes", () => ran = true);

        // Act & Assert: a failing step propagates its exception.
        var ex = Assert.Throws<InvalidOperationException>(() => reporter.Step("fails", () => throw new InvalidOperationException("boom")));

        // Assert: the successful step ran and the failure message was preserved.
        Assert.True(ran);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task ConsoleReporter_Should_RunAsyncStepBodies_And_LetFailuresThrough()
    {
        // Arrange
        var reporter = new ConsoleTestReporter();
        var ran = false;

        // Act
        await reporter.StepAsync("passes", async () => { await Task.Yield(); ran = true; });

        // Act & Assert: a failing asynchronous step propagates its exception.
        await Assert.ThrowsAsync<TimeoutException>(() => reporter.StepAsync("fails", () => throw new TimeoutException()));

        // Assert: the successful step ran.
        Assert.True(ran);
    }

    [Fact]
    public void ConsoleReporter_Should_IgnoreNames_Parameters_And_Attachments()
    {
        // Arrange
        var reporter = new ConsoleTestReporter();

        // Act
        reporter.BeginRun();
        reporter.UpdateTestName(name => name + " (x)");
        reporter.AddParameter("browser", "chrome");
        reporter.Attach("Test Failure", "image/png", [1, 2, 3], "png");

        // Assert: reaching this point without an exception confirms these calls are harmless.
    }

    [Fact]
    public async Task ReportHelper_Should_RunTheStep_WithTheSelectedReporter()
    {
        // Arrange
        var steps = new List<string>();

        // Act
        Report.Step("sync", () => steps.Add("sync"));
        await Report.Step("async", async () => { await Task.Yield(); steps.Add("async"); });

        // Assert
        Assert.Equal(["sync", "async"], steps);
    }

    [Fact]
    public void Reporter_Should_BeTheSelectedOne_When_Registered()
    {
        // Arrange
        var registry = Framework.CreateRegistry([new AllureReportingPlugin()]);

        // Act & Assert
        Assert.IsType<AllureTestReporter>(Framework.ResolveReporter(registry, "allure"));
        Assert.IsType<ConsoleTestReporter>(Framework.ResolveReporter(registry, "console"));
    }

    [Fact]
    public void Reporter_Should_FallBackToConsole_When_NotRegistered()
    {
        // Arrange: use the built-in registry without an Allure plugin.
        // Act & Assert
        Assert.IsType<ConsoleTestReporter>(Framework.ResolveReporter(Framework.CreateRegistry([]), "allure"));
    }

    [Fact]
    public void Reporting_Should_DefaultTo_Console()
    {
        // Arrange: use the default provider settings.
        // Act & Assert
        Assert.Equal("console", new ProvidersSettings().Reporting);
    }
}
