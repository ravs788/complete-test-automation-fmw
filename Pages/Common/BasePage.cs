using Microsoft.Playwright;

namespace Pages.Common;

public abstract class BasePage(IPage page)
{
    /// <summary>Extra attempts for a navigation that fails with a network error.</summary>
    private const int NavigationRetries = 2;

    protected readonly IPage Page = page ?? throw new ArgumentNullException(nameof(page));

    /// <summary>
    /// The attribute the app uses for test ids (Playwright's default is <c>data-testid</c>). Override it in the
    /// app's base page, e.g. SauceDemo uses <c>data-test</c>.
    /// </summary>
    protected virtual string TestIdAttribute => "data-testid";

    /// <summary>
    /// Locates an element by its test id. Prefer this, or <c>GetByRole</c> / <c>GetByLabel</c> / <c>GetByPlaceholder</c>,
    /// over CSS classes, which change with styling.
    /// </summary>
    protected ILocator TestId(string id) => Page.Locator($"[{TestIdAttribute}=\"{id}\"]");

    /// <summary>
    /// Navigates to <paramref name="url"/>, retrying up to <see cref="NavigationRetries"/> times when the browser
    /// reports a network error (<c>net::ERR_*</c>, e.g. a connection reset by a busy demo site). Other failures,
    /// and all assertions, are not retried.
    /// </summary>
    protected async Task<IResponse?> GotoAsync(string url)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await Page.GotoAsync(url);
            }
            catch (PlaywrightException ex) when (attempt < NavigationRetries && ex.Message.Contains("net::ERR_", StringComparison.Ordinal))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1)));
            }
        }
    }
}
