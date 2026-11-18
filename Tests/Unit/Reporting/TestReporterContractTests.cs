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
        // Arrange
        var ran = false;

        // Act
        Reporter.Step("passes", () => ran = true);

        // Act & Assert: a failing step propagates its exception.
        var ex = Assert.Throws<InvalidOperationException>(() => Reporter.Step("fails", () => throw new InvalidOperationException("boom")));

        // Assert: the successful step ran and the failure message was preserved.
        Assert.True(ran);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task StepAsync_Should_RunTheBody_And_LetFailuresThrough()
    {
        // Arrange
        var ran = false;

        // Act
        await Reporter.StepAsync("passes", async () => { await Task.Yield(); ran = true; });

        // Act & Assert: a failing asynchronous step propagates its exception.
        await Assert.ThrowsAsync<TimeoutException>(() => Reporter.StepAsync("fails", () => throw new TimeoutException()));

        // Assert: the successful step ran.
        Assert.True(ran);
    }

    [Fact]
    public void BestEffortCalls_Should_NeverThrow_OutsideAReportedTest()
    {
        // Arrange: use the reporter supplied by the contract implementation.
        // Act
        Reporter.UpdateTestName(name => name + " (x)");
        Reporter.SetSuite("Layer", "App", "Feature", ["tag"]);
        Reporter.AddParameter("browser", "chrome");
        Reporter.Attach("Test Failure", "image/png", [1, 2, 3], "png");

        // Assert: reaching this point without an exception satisfies the best-effort contract.
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
