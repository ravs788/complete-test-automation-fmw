using Microsoft.Playwright;

namespace Core.Http.Playwright;

/// <summary>
/// Creates <see cref="PlaywrightHttpTransport"/>s. One Playwright instance (a Node driver process) is shared by the
/// whole run; each transport gets its own request context, which it disposes.
/// </summary>
public sealed class PlaywrightHttpTransportFactory : IHttpTransportFactory
{
    private static readonly Lazy<Task<IPlaywright>> SharedPlaywright = new(StartAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task<IHttpTransport> CreateAsync(string baseUrl, IReadOnlyDictionary<string, string> defaultHeaders)
    {
        var playwright = await SharedPlaywright.Value;
        var context = await playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = baseUrl,
            ExtraHTTPHeaders = defaultHeaders,
        });
        return new PlaywrightHttpTransport(context);
    }

    private static async Task<IPlaywright> StartAsync()
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => playwright.Dispose();
        return playwright;
    }
}
