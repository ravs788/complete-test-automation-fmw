using System.Runtime.CompilerServices;
using Allure.Net.Commons;
using Core.Configuration;
using Core.Logging;
using Core.Testing;
using Microsoft.Playwright;
using Tests.Fixtures;

namespace Tests.Infrastructure;

/// <summary>
/// Base for browser tests. Each test gets a fresh context and page (opened at <see cref="BaseUrl"/>);
/// wrap the test body in <see cref="RunAsync"/> for logging, Allure timing, a full-page screenshot and a
/// Playwright trace on failure (see <see cref="TestSettings.Trace"/>) and results publishing (via Core's
/// <see cref="TestRunner"/>).
/// </summary>
public abstract class UiTestBase : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    protected UiTestBase(PlaywrightFixture fixture)
    {
        Fixture = fixture;
        Logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
    }

    protected PlaywrightFixture Fixture { get; }
    protected TestSettings Settings => Fixture.Settings;
    protected ILoggingService Logger { get; }
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    /// <summary>Base URL of the application under test (the context's BaseURL).</summary>
    protected abstract string BaseUrl { get; }

    private bool Tracing => !string.Equals(Settings.Trace, TraceModes.Off, StringComparison.OrdinalIgnoreCase);
    private bool _traceSaved;

    public async Task InitializeAsync()
    {
        (Context, Page) = await Fixture.CreateCleanPageAsync(BaseUrl);
        if (Tracing)
        {
            await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        }
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.CloseAsync();
        }
    }

    protected Task RunAsync(Func<Task> test, [CallerMemberName] string testName = "") =>
        TestRunner.RunAsync(
            new TestRunInfo(Logger, "ui-web", GetType(), testName, Settings.Browser),
            body: async () =>
            {
                // Show the browser in the Allure test name, e.g. "Checkout_Should_CompleteOrder (chrome)", and keep
                // runs on different browsers apart when their results are merged (run_ui_web_all_browsers).
                AllureLifecycle.Instance.UpdateTestCase(tc =>
                {
                    tc.name = $"{tc.name} ({Settings.Browser})";
                    tc.parameters.Add(new Parameter { name = "browser", value = Settings.Browser });
                });
                try
                {
                    await test();
                }
                catch (PlaywrightException ex)
                {
                    // Playwright's Expect(...) throws PlaywrightException, which Allure reports as "broken"
                    // (it only treats xUnit's own assertion exceptions as "failed"). Re-raise it as an xUnit
                    // failure, keeping Playwright's message (with its call log) and stack trace.
                    Assert.Fail($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            },
            onFailureAsync: AttachFailureArtifactsAsync,
            cleanupAsync: AttachTraceIfAlwaysOnAsync);

    private async Task AttachFailureArtifactsAsync()
    {
        var png = await Page.ScreenshotAsync(new() { FullPage = true });
        AllureApi.AddAttachment("Test Failure", "image/png", png, $"failure_{DateTime.Now:yyyyMMddHHmmssfff}.png");

        if (Tracing)
        {
            await AttachTraceAsync();
        }
    }

    private async Task AttachTraceIfAlwaysOnAsync()
    {
        if (!_traceSaved && string.Equals(Settings.Trace, TraceModes.On, StringComparison.OrdinalIgnoreCase))
        {
            await AttachTraceAsync();
        }
    }

    /// <summary>
    /// Stops tracing into a zip and attaches it to Allure. Allure's report opens it in the Playwright trace viewer;
    /// it can also be opened with <c>pwsh playwright.ps1 show-trace &lt;file&gt;</c> or at https://trace.playwright.dev.
    /// </summary>
    private async Task AttachTraceAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"trace_{Guid.NewGuid():N}.zip");
        try
        {
            await Context.Tracing.StopAsync(new() { Path = path });
            _traceSaved = true;
            AllureApi.AddAttachment("Playwright trace", "application/vnd.allure.playwright-trace", await File.ReadAllBytesAsync(path), "zip");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
