using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Core.Ui.Playwright;

/// <summary><see cref="IElement"/> on a Playwright <see cref="ILocator"/> (auto-waiting).</summary>
public sealed class PlaywrightElement(IPage page, ILocator locator, string description) : IElement
{
    public string Description => description;

    public IElement Find(Locate locate) => new PlaywrightElement(page, Resolve(locator, locate), $"{description} >> {locate}");

    public IElement WithChild(Locate child, string text) => new PlaywrightElement(
        page,
        locator.Filter(new() { Has = Resolve(page, child).Filter(new() { HasTextString = text }) }),
        $"{description} with {child} containing \"{text}\"");

    public IElement First => new PlaywrightElement(page, locator.First, $"first of {description}");

    public Task ClickAsync() => PlaywrightErrors.Wrap($"click {description}", () => locator.ClickAsync());

    public Task FillAsync(string text) => PlaywrightErrors.Wrap($"fill {description}", () => locator.FillAsync(text));

    public Task PressAsync(string key) => PlaywrightErrors.Wrap($"press {key} on {description}", () => locator.PressAsync(key));

    public Task CheckAsync() => PlaywrightErrors.Wrap($"check {description}", () => locator.CheckAsync());

    public async Task<string> TextAsync() =>
        await PlaywrightErrors.Wrap($"read text of {description}", () => locator.InnerTextAsync());

    public Task<int> CountAsync() => PlaywrightErrors.Wrap($"count {description}", () => locator.CountAsync());

    public Task<bool> IsVisibleAsync() => PlaywrightErrors.Wrap($"check visibility of {description}", () => locator.IsVisibleAsync());

    public IElementAssertions Expect() => new PlaywrightElementAssertions(locator, description);

    /// <summary>Turns a framework <see cref="Locate"/> into a Playwright locator under <paramref name="root"/>.</summary>
    internal static ILocator Resolve(IPage root, Locate locate) => locate.Kind switch
    {
        LocateKind.Role => root.GetByRole(Enum.Parse<AriaRole>(locate.Value), new() { Name = locate.Name }),
        LocateKind.Placeholder => root.GetByPlaceholder(locate.Value),
        _ => root.Locator(locate.Value),
    };

    internal static ILocator Resolve(ILocator root, Locate locate) => locate.Kind switch
    {
        LocateKind.Role => root.GetByRole(Enum.Parse<AriaRole>(locate.Value), new() { Name = locate.Name }),
        LocateKind.Placeholder => root.GetByPlaceholder(locate.Value),
        _ => root.Locator(locate.Value),
    };
}

/// <summary><see cref="IElementAssertions"/> on Playwright's web-first <c>Expect(locator)</c> assertions.</summary>
public sealed class PlaywrightElementAssertions(ILocator locator, string description) : IElementAssertions
{
    public Task ToBeVisibleAsync() => Assert($"{description} to be visible", a => a.ToBeVisibleAsync());

    public Task ToHaveTextAsync(string text) => Assert($"{description} to have text \"{text}\"", a => a.ToHaveTextAsync(text));

    public Task ToHaveTextAsync(IReadOnlyList<string> texts) =>
        Assert($"{description} to have texts [{string.Join(", ", texts)}]", a => a.ToHaveTextAsync(texts.ToArray()));

    public Task ToContainTextAsync(string text, bool ignoreCase = false) =>
        Assert($"{description} to contain \"{text}\"", a => a.ToContainTextAsync(text, new() { IgnoreCase = ignoreCase }));

    public Task ToHaveCountAsync(int count) => Assert($"{description} to have count {count}", a => a.ToHaveCountAsync(count));

    public Task ToHaveClassAsync(string className) =>
        Assert($"{description} to have class \"{className}\"", a => a.ToHaveClassAsync(new Regex($"(^|\\s){Regex.Escape(className)}(\\s|$)")));

    private async Task Assert(string expectation, Func<ILocatorAssertions, Task> assertion)
    {
        try
        {
            await assertion(Assertions.Expect(locator));
        }
        catch (Exception ex) when (ex is PlaywrightException or TimeoutException)
        {
            throw new UiAssertionException($"Expected {expectation}.{Environment.NewLine}{ex.Message}", ex);
        }
    }
}

/// <summary>
/// Maps Playwright failures (<see cref="PlaywrightException"/>, and <see cref="TimeoutException"/> when an element
/// never becomes actionable) to the framework's <see cref="UiException"/>, keeping Playwright's message (with its call log).
/// </summary>
internal static class PlaywrightErrors
{
    public static async Task Wrap(string action, Func<Task> body)
    {
        try
        {
            await body();
        }
        catch (Exception ex) when (ex is PlaywrightException or TimeoutException)
        {
            throw new UiException($"Could not {action}.{Environment.NewLine}{ex.Message}", ex);
        }
    }

    public static async Task<T> Wrap<T>(string action, Func<Task<T>> body)
    {
        try
        {
            return await body();
        }
        catch (Exception ex) when (ex is PlaywrightException or TimeoutException)
        {
            throw new UiException($"Could not {action}.{Environment.NewLine}{ex.Message}", ex);
        }
    }
}
