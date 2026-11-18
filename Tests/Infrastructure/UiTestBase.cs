using System.Runtime.CompilerServices;
using Core.Composition;
using Core.Configuration;
using Core.Logging;
using Core.Reporting;
using Core.Security;
using Core.Testing;
using Core.Ui;
using Core.Ui.Capabilities;
using Tests.Fixtures;

namespace Tests.Infrastructure;

/// <summary>
/// Base for browser tests. Each test gets a fresh <see cref="Session"/> (opened at <see cref="BaseUrl"/>) from the UI
/// driver selected in <c>providers.json</c> (R42); wrap the test body in <see cref="RunAsync"/> for logging, report
/// timing, a full-page screenshot and a trace on failure (when the driver supports them, see
/// <see cref="TestSettings.Trace"/>) and results publishing (via Core's <see cref="TestRunner"/>).
/// </summary>
public abstract class UiTestBase : IClassFixture<BrowserFixture>, IAsyncLifetime
{
    protected UiTestBase(BrowserFixture fixture)
    {
        Fixture = fixture;
        Logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
    }

    protected BrowserFixture Fixture { get; }
    protected TestSettings Settings => Fixture.Settings;
    protected ILoggingService Logger { get; }

    /// <summary>This test's browser session (isolated cookies and storage, one page).</summary>
    protected IUiSession Session { get; private set; } = null!;

    /// <summary>Base URL of the application under test (the session's base URL).</summary>
    protected abstract string BaseUrl { get; }

    private bool Tracing => DiagnosticArtifactPolicy.Current.AllowAttachments && !string.Equals(Settings.Trace, TraceModes.Off, StringComparison.OrdinalIgnoreCase);
    private bool _traceSaved;
    private bool _traceStarted;

    public async Task InitializeAsync()
    {
        Session = await Fixture.NewSessionAsync(BaseUrl);
        if (Tracing && Session.TryGet<ITracing>(out var tracing))
        {
            await tracing.StartAsync();
            _traceStarted = true;
        }
    }

    public async Task DisposeAsync()
    {
        if (Session != null)
        {
            await Session.DisposeAsync();
        }
    }

    protected Task RunAsync(Func<Task> test, [CallerMemberName] string testName = "") =>
        TestRunner.RunAsync(
            new TestRunInfo(Logger, "ui-web", GetType(), testName, Settings.Browser),
            body: async () =>
            {
                // Show the browser in the report's test name, e.g. "Checkout_Should_CompleteOrder (chrome)", and keep
                // runs on different browsers apart when their results are merged (run_ui_web_all_browsers).
                Framework.Reporter.UpdateTestName(name => $"{name} ({Settings.Browser})");
                Framework.Reporter.AddParameter("browser", Settings.Browser);
                try
                {
                    await test();
                }
                catch (UiException ex)
                {
                    // UI failures (a missed assertion, an element that never appeared) are test failures: report them
                    // as xUnit assertion failures so Allure shows "failed" rather than "broken", keeping the driver's
                    // message (with its call log) and stack trace.
                    Assert.Fail(DiagnosticRedactor.Redact($"{ex.Message}{Environment.NewLine}{ex.InnerException?.StackTrace ?? ex.StackTrace}"));
                }
            },
            onFailureAsync: AttachFailureArtifactsAsync,
            cleanupAsync: AttachTraceIfAlwaysOnAsync);

    private async Task AttachFailureArtifactsAsync()
    {
        try
        {
            if (DiagnosticArtifactPolicy.Current.AllowAttachments && Session.TryGet<IScreenshots>(out var screenshots))
            {
                await DiagnosticArtifactPolicy.Current.CaptureAsync(
                    () => screenshots.CaptureAsync(fullPage: true),
                    png => Framework.Reporter.Attach("Test Failure", "image/png", png, $"failure_{DateTime.Now:yyyyMMddHHmmssfff}.png"));
            }
        }
        finally
        {
            // A screenshot failure must not prevent saving an otherwise usable trace.
            if (Tracing)
            {
                await AttachTraceAsync();
            }
        }
    }

    private async Task AttachTraceIfAlwaysOnAsync()
    {
        if (_traceStarted && !_traceSaved && string.Equals(Settings.Trace, TraceModes.On, StringComparison.OrdinalIgnoreCase))
        {
            await AttachTraceAsync();
        }
    }

    /// <summary>
    /// Stops tracing and attaches the trace to the report (Allure opens Playwright traces in Playwright's trace viewer:
    /// a step-by-step replay with DOM snapshots, network and console).
    /// </summary>
    private async Task AttachTraceAsync()
    {
        if (!_traceStarted || !Session.TryGet<ITracing>(out var tracing))
        {
            return;
        }
        var trace = await tracing.StopAsync();
        _traceSaved = true;
        _traceStarted = false;
        Framework.Reporter.Attach(tracing.TraceName, tracing.TraceContentType, trace, tracing.TraceFileExtension);
    }
}
