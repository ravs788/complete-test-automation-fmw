using System.Diagnostics;
using Allure.Net.Commons;

namespace Core.Utilities
{
    /// <summary>
    /// Runs a test body with the bookkeeping every suite needs: timing, start/finish logging,
    /// Allure timing parameters, results publishing, a failure hook (e.g. screenshot) and cleanup.
    /// </summary>
    /// <remarks>
    /// xUnit v2 has no SetUp/TearDown with access to the test's name or outcome, so base test
    /// classes wrap each test body in this instead. Framework-agnostic: no xUnit dependency.
    /// </remarks>
    public static class TestRunner
    {
        public static void Run(TestRunInfo info, Action body, Action? onFailure = null, Action? cleanup = null) =>
            RunAsync(info, () => { body(); return Task.CompletedTask; }, onFailure, cleanup).GetAwaiter().GetResult();

        public static async Task RunAsync(TestRunInfo info, Func<Task> body, Action? onFailure = null, Action? cleanup = null)
        {
            var logger = info.Logger;
            var startTime = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();
            var status = "Passed";
            var reason = string.Empty;
            var target = string.IsNullOrEmpty(info.Browser) ? string.Empty : $" on '{info.Browser}'";

            logger.Info($"[Start] {info.TestClass.Name}.{info.TestName}{target}");
            try
            {
                await body();
            }
            catch (Exception ex)
            {
                status = "Failed";
                reason = ex.Message;
                logger.Error($"[Failed] {info.TestClass.Name}.{info.TestName}{target}: {ex.Message}");
                try { onFailure?.Invoke(); } catch { /* never mask the original failure */ }
                throw;
            }
            finally
            {
                var endTime = DateTime.Now;
                var duration = stopwatch.Elapsed.TotalSeconds.ToString("F3");
                logger.Info($"[Finish] {info.TestClass.Name}.{info.TestName}{target} | Outcome: {status} | Duration(s): {duration}");

                TryAddAllureTiming(startTime, endTime, duration);
                Publish(info, status, reason, startTime, endTime, duration);

                try { cleanup?.Invoke(); }
                catch (Exception ex) { logger.Error($"[Cleanup] {ex.Message}"); }
            }
        }

        private static void TryAddAllureTiming(DateTime startTime, DateTime endTime, string duration)
        {
            try
            {
                AllureLifecycle.Instance.UpdateTestCase(tc =>
                {
                    tc.parameters.Add(new Parameter { name = "Start Time", value = startTime.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                    tc.parameters.Add(new Parameter { name = "End Time", value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                    tc.parameters.Add(new Parameter { name = "Duration (s)", value = duration });
                });
            }
            catch
            {
                // No active Allure test case (e.g. Allure not loaded); reporting is best effort.
            }
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
                info.Logger.Error($"[Results] Publish failed: {ex.Message}");
            }
        }
    }

    /// <summary>Identifies the test being run by <see cref="TestRunner"/>.</summary>
    public sealed record TestRunInfo(
        ILoggingService Logger,
        string ProjectName,
        Type TestClass,
        string TestName,
        string Browser = "");
}
