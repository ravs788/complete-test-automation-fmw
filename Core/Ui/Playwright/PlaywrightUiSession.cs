using Core.Ui.Capabilities;
using Microsoft.Playwright;

namespace Core.Ui.Playwright;

/// <summary><see cref="IUiSession"/> on a Playwright browser context and page; supports screenshots and tracing.</summary>
public sealed class PlaywrightUiSession(IBrowserContext context, IPage page) : IUiSession, IScreenshots, Capabilities.ITracing
{
    public Task NavigateAsync(string url) =>
        PlaywrightErrors.Wrap($"navigate to {url}", () => page.GotoAsync(url));

    public IElement Find(Locate locate) => new PlaywrightElement(page, PlaywrightElement.Resolve(page, locate), locate.ToString());

    public bool TryGet<TCapability>(out TCapability capability) where TCapability : class
    {
        capability = (this as TCapability)!;
        return capability is not null;
    }

    public T Native<T>() where T : class =>
        page as T ?? context as T ?? throw new NotSupportedException($"The Playwright session has no {typeof(T).Name}; use IPage or IBrowserContext.");

    public Task<byte[]> CaptureAsync(bool fullPage = true) => page.ScreenshotAsync(new() { FullPage = fullPage });

    public string TraceName => "Playwright trace";

    /// <summary>Allure opens this content type in Playwright's trace viewer.</summary>
    public string TraceContentType => "application/vnd.allure.playwright-trace";

    public string TraceFileExtension => "zip";

    public Task StartAsync() => context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });

    public async Task<byte[]> StopAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"trace_{Guid.NewGuid():N}.zip");
        try
        {
            await context.Tracing.StopAsync(new() { Path = path });
            return await File.ReadAllBytesAsync(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    public ValueTask DisposeAsync() => new(context.CloseAsync());
}
