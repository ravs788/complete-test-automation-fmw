using Core.Reporting;
using Core.Reporting.Allure;

namespace Tests.Unit.Reporting;

/// <summary>
/// The behaviour every <see cref="ITestReporter"/> must have (R39, R46): steps run their body and let its failures
/// through; everything else is best effort and never throws, even outside a reported test.
/// </summary>
public abstract class TestReporterContractTests
{
    protected abstract ITestReporter Reporter { get; }

    [Fact]
    public void Step_Should_RunTheBody_And_LetFailuresThrough()
    {
        var ran = false;

        Reporter.Step("passes", () => ran = true);
        var ex = Assert.Throws<InvalidOperationException>(() => Reporter.Step("fails", () => throw new InvalidOperationException("boom")));

        Assert.True(ran);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task StepAsync_Should_RunTheBody_And_LetFailuresThrough()
    {
        var ran = false;

        await Reporter.StepAsync("passes", async () => { await Task.Yield(); ran = true; });
        await Assert.ThrowsAsync<TimeoutException>(() => Reporter.StepAsync("fails", () => throw new TimeoutException()));

        Assert.True(ran);
    }

    [Fact]
    public void BestEffortCalls_Should_NeverThrow_OutsideAReportedTest()
    {
        Reporter.UpdateTestName(name => name + " (x)");
        Reporter.SetSuite("Layer", "App", "Feature", ["tag"]);
        Reporter.AddParameter("browser", "chrome");
        Reporter.Attach("Test Failure", "image/png", [1, 2, 3], "png");
    }
}

public sealed class ConsoleTestReporterContractTests : TestReporterContractTests
{
    protected override ITestReporter Reporter { get; } = new ConsoleTestReporter();
}

public sealed class AllureTestReporterContractTests : TestReporterContractTests
{
    protected override ITestReporter Reporter { get; } = new AllureTestReporter();
}
