using System.Diagnostics;
using System.Reflection;
using Core.Composition;
using Core.Logging;
using Core.Reporting;
using Core.Security;

namespace Core.Testing;

/// <summary>
/// Runs a test body with the bookkeeping every suite needs: timing, start/finish logging,
/// timing parameters for the report (ITestReporter), results publishing, a failure hook (e.g. screenshot) and cleanup.
/// </summary>
/// <remarks>
/// xUnit v2 has no SetUp/TearDown with access to the test's name or outcome, so base test
/// classes wrap each test body in this instead. Framework-agnostic: no xUnit dependency.
/// </remarks>
public static class TestRunner
{
    public static void Run(TestRunInfo info, Action body, Action? onFailure = null, Action? cleanup = null) =>
        RunAsync(info, () => { body(); return Task.CompletedTask; }, onFailure, cleanup).GetAwaiter().GetResult();

    public static Task RunAsync(TestRunInfo info, Func<Task> body, Action? onFailure = null, Action? cleanup = null) =>
        RunAsync(
            info,
            body,
            onFailureAsync: () => { onFailure?.Invoke(); return Task.CompletedTask; },
            cleanupAsync: () => { cleanup?.Invoke(); return Task.CompletedTask; });

    /// <summary>Async hooks, e.g. a Playwright screenshot on failure.</summary>
    public static async Task RunAsync(TestRunInfo info, Func<Task> body, Func<Task> onFailureAsync, Func<Task>? cleanupAsync = null)
    {
        var logger = info.Logger;
        var startTime = DateTime.Now;
        var stopwatch = Stopwatch.StartNew();
        var status = "Passed";
        var reason = string.Empty;
        var target = string.IsNullOrEmpty(info.Browser) ? string.Empty : $" on '{info.Browser}'";

        logger.Info($"[Start] {info.TestClass.Name}.{info.TestName}{target}");
        ApplyReportLabels(info.TestClass);
        try
        {
            await body();
        }
        catch (Exception ex)
        {
            status = "Failed";
            reason = DiagnosticRedactor.Redact(ex.Message);
            logger.Error($"[Failed] {info.TestClass.Name}.{info.TestName}{target}: {reason}");
            try { await onFailureAsync(); } catch { /* never mask the original failure */ }
            if (DiagnosticRedactor.Redact(ex.ToString()) != ex.ToString()) throw new RedactedTestException(ex);
            throw;
        }
        finally
        {
            var endTime = DateTime.Now;
            var duration = stopwatch.Elapsed.TotalSeconds.ToString("F3");
            logger.Info($"[Finish] {info.TestClass.Name}.{info.TestName}{target} | Outcome: {status} | Duration(s): {duration}");

            AddTiming(startTime, endTime, duration);
            Publish(info, status, reason, startTime, endTime, duration);

            try { if (cleanupAsync is not null) await cleanupAsync(); }
            catch (Exception ex) { logger.Error(DiagnosticRedactor.Redact($"[Cleanup] {ex.Message}")); }
        }
    }

    /// <summary>Passes the class's <see cref="ReportSuiteAttribute"/> / <see cref="ReportTagsAttribute"/> to the reporter (R48).</summary>
    private static void ApplyReportLabels(Type testClass)
    {
        if (testClass.GetCustomAttribute<ReportSuiteAttribute>() is { } suite)
        {
            var tags = testClass.GetCustomAttribute<ReportTagsAttribute>()?.Tags ?? [];
            Framework.Reporter.SetSuite(suite.ParentSuite, suite.Suite, suite.SubSuite, tags);
        }
    }

    private static void AddTiming(DateTime startTime, DateTime endTime, string duration)
    {
        // Reporters are best effort (ITestReporter): outside a reported test these calls do nothing.
        var reporter = Framework.Reporter;
        reporter.AddParameter("Start Time", startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        reporter.AddParameter("End Time", endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        reporter.AddParameter("Duration (s)", duration);
    }

    private static void Publish(TestRunInfo info, string status, string reason, DateTime startTime, DateTime endTime, string duration)
    {
        try
        {
            ResultsPublisherFactory.Create().Publish(new LogMetadata
            {
                ProjectName = info.ProjectName,
                TestClassName = info.TestClass.FullName ?? info.TestClass.Name,
                TestMethodName = info.TestName,
                Status = status,
                Duration = duration,
                Reason = reason,
                RunTime = endTime.ToString("o"),
                RunName = $"{info.TestClass.FullName}.{info.TestName}",
                TriggeredBy = Environment.UserName,
                Browser = info.Browser,
                StartTime = startTime,
                EndTime = endTime
            });
        }
        catch (Exception ex)
        {
            info.Logger.Error(DiagnosticRedactor.Redact($"[Results] Publish failed: {ex.Message}"));
        }
    }
}
