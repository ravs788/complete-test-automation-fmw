using Microsoft.Playwright;

namespace Core.Http.Playwright;

/// <summary>
/// Creates <see cref="PlaywrightHttpTransport"/>s. One Playwright instance (a Node driver process) is shared by the
/// whole run; each request gets an independently owned context so cancelling it cannot abort another request.
/// </summary>
public sealed class PlaywrightHttpTransportFactory : IHttpTransportFactory
{
    private static readonly Lazy<Task<IPlaywright>> SharedPlaywright = new(StartAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task<IHttpTransport> CreateAsync(string baseUrl, IReadOnlyDictionary<string, string> defaultHeaders)
    {
        var playwright = await SharedPlaywright.Value;
        return new PlaywrightHttpTransport(playwright, baseUrl, defaultHeaders);
    }

    private static async Task<IPlaywright> StartAsync()
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => playwright.Dispose();
        return playwright;
    }
}
