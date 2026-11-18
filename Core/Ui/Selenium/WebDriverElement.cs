using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace Core.Ui.Selenium;

/// <summary>
/// <see cref="IElement"/> on WebDriver: a lazy query re-run on every action, which waits (polling) up to the session
/// timeout for a displayed, enabled element and retries transient failures (stale, covered, not yet interactable).
/// </summary>
public sealed class WebDriverElement : IElement
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    private readonly Func<IReadOnlyList<IWebElement>> _resolve;
    private readonly TimeSpan _timeout;

    internal WebDriverElement(Func<IReadOnlyList<IWebElement>> resolve, string description, TimeSpan timeout)
    {
        _resolve = resolve;
        Description = description;
        _timeout = timeout;
    }

    public string Description { get; }

    public IElement Find(Locate locate) =>
        new WebDriverElement(() => _resolve().SelectMany(e => WebDriverLocate.Query(e, locate)).ToList(), $"{Description} >> {locate}", _timeout);

    public IElement WithChild(Locate child, string text) => new WebDriverElement(
        () => _resolve().Where(e => WebDriverLocate.Query(e, child).Any(c => WebDriverLocate.Contains(c.Text, text))).ToList(),
        $"{Description} with {child} containing \"{text}\"",
        _timeout);

    public IElement First => new WebDriverElement(() => _resolve().Take(1).ToList(), $"first of {Description}", _timeout);

    public Task ClickAsync() => ActAsync("click", e => e.Click(), waitUntilStable: true);

    public Task FillAsync(string text) => ActAsync("fill", e => { e.Clear(); e.SendKeys(text); });

    public Task PressAsync(string key) => ActAsync($"press {key} on", e => e.SendKeys(KeyFor(key)));

    public Task CheckAsync() => ActAsync("check", e =>
    {
        var type = e.GetDomAttribute("type");
        Func<bool> isChecked;
        if (string.Equals(e.TagName, "input", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(type, "checkbox", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "radio", StringComparison.OrdinalIgnoreCase)))
        {
            isChecked = () => e.Selected;
        }
        else if (e.GetDomAttribute("role") is "checkbox" or "radio")
        {
            isChecked = () => string.Equals(e.GetDomAttribute("aria-checked"), "true", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            throw new UiException($"Cannot check {Description}: expected a checkbox or radio element.");
        }
        if (!isChecked())
        {
            if (!e.Enabled || string.Equals(e.GetDomAttribute("aria-disabled"), "true", StringComparison.OrdinalIgnoreCase))
            {
                throw new ElementNotInteractableException($"Element is disabled: {Description}.");
            }
            e.Click();
            if (!isChecked())
            {
                throw new UiException($"Could not check {Description}: element remained unchecked after clicking.");
            }
        }
    }, requireEnabled: false, waitUntilStable: true);

    public async Task<string> TextAsync()
    {
        string text = "";
        await ActAsync("read text of", e => text = e.Text ?? "", requireEnabled: false);
        return text;
    }

    public Task<int> CountAsync() => Task.FromResult(WebDriverErrors.Run($"count {Description}", () => _resolve().Count));

    public Task<bool> IsVisibleAsync()
    {
        try
        {
            var elements = _resolve();
            EnsureSingle(elements.Count);
            return Task.FromResult(elements.Count == 1 && elements[0].Displayed);
        }
        catch (StaleElementReferenceException)
        {
            return Task.FromResult(false);
        }
    }

    public IElementAssertions Expect() => new WebDriverAssertions(_resolve, Description);

    /// <summary>
    /// Requires an unambiguous locator, optionally waits until the element stops moving, and acts on it; retries transient
    /// failures until the timeout.
    /// </summary>
    private async Task ActAsync(string action, Action<IWebElement> act, bool requireEnabled = true, bool waitUntilStable = false)
    {
        var deadline = DateTime.UtcNow + _timeout;
        while (true)
        {
            try
            {
                var candidates = _resolve();
                EnsureSingle(candidates.Count);
                var element = candidates.FirstOrDefault() ?? throw new NoSuchElementException($"No element for {Description}.");
                if (requireEnabled && !element.Enabled)
                {
                    throw new ElementNotInteractableException($"Element is disabled: {Description}.");
                }
                if (waitUntilStable)
                {
                    await WaitUntilStableAsync(element);
                }
                act(element);
                return;
            }
            catch (WebDriverException ex) when (WebDriverErrors.IsTransient(ex))
            {
                if (DateTime.UtcNow >= deadline)
                {
                    throw new UiException($"Could not {action} {Description} within {_timeout.TotalSeconds:F0} s.{Environment.NewLine}{ex.Message}", ex);
                }
                await Task.Delay(PollInterval);
            }
            catch (WebDriverException ex)
            {
                throw new UiException($"Could not {action} {Description}.{Environment.NewLine}{ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Like Playwright's "stable" check: waits until the element has the same position and size in two reads 50 ms
    /// apart, so a click doesn't land on an element that is being re-rendered or animated. A detached element throws
    /// <see cref="StaleElementReferenceException"/>, which the caller retries with a fresh lookup.
    /// </summary>
    private static async Task WaitUntilStableAsync(IWebElement element)
    {
        var previous = (element.Location, element.Size);
        for (var i = 0; i < 20; i++)
        {
            await Task.Delay(50);
            var current = (element.Location, element.Size);
            if (current == previous)
            {
                return;
            }
            previous = current;
        }
    }

    private void EnsureSingle(int count)
    {
        if (count > 1)
        {
            throw new UiException($"Ambiguous locator {Description}: matched {count} elements. Refine the locator or explicitly use First.");
        }
    }

    private static string KeyFor(string key) => key switch
    {
        "Enter" => Keys.Enter,
        "Tab" => Keys.Tab,
        "Escape" => Keys.Escape,
        "Backspace" => Keys.Backspace,
        "ArrowDown" => Keys.ArrowDown,
        "ArrowUp" => Keys.ArrowUp,
        "ArrowLeft" => Keys.ArrowLeft,
        "ArrowRight" => Keys.ArrowRight,
        _ => key,
    };
}

/// <summary>
/// <see cref="IElementAssertions"/> for WebDriver: polls every 100 ms until the expectation holds or 5 s pass (the same
/// assertion timeout as Playwright's), then throws <see cref="UiAssertionException"/> with the last value seen.
/// </summary>
public sealed class WebDriverAssertions : IElementAssertions
{
    private static readonly TimeSpan AssertTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    private readonly Func<IReadOnlyList<IWebElement>> _resolve;
    private readonly string _description;

    internal WebDriverAssertions(Func<IReadOnlyList<IWebElement>> resolve, string description)
    {
        _resolve = resolve;
        _description = description;
    }

    public Task ToBeVisibleAsync() =>
        AssertAsync("to be visible", els => els.Any(e => e.Displayed), els => $"{els.Count} element(s), visible: {els.Count(e => e.Displayed)}");

    public Task ToHaveTextAsync(string text) =>
        AssertAsync($"to have text \"{text}\"", els => els.Count > 0 && Normalize(els[0].Text) == Normalize(text), FirstText);

    public Task ToHaveTextAsync(IReadOnlyList<string> texts) =>
        AssertAsync(
            $"to have texts [{string.Join(", ", texts)}]",
            els => els.Select(e => Normalize(e.Text)).SequenceEqual(texts.Select(Normalize)),
            els => $"[{string.Join(", ", els.Select(e => Normalize(e.Text)))}]", single: false);

    public Task ToContainTextAsync(string text, bool ignoreCase = false) =>
        AssertAsync(
            $"to contain \"{text}\"",
            els => els.Count > 0 && Normalize(els[0].Text).Contains(text, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal),
            FirstText);

    public Task ToHaveCountAsync(int count) => AssertAsync($"to have count {count}", els => els.Count == count, els => $"count {els.Count}", single: false);

    public Task ToHaveClassAsync(string className) =>
        AssertAsync(
            $"to have class \"{className}\"",
            els => els.Count > 0 && (els[0].GetAttribute("class") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(className),
            els => els.Count > 0 ? $"class \"{els[0].GetAttribute("class")}\"" : "no element");

    private async Task AssertAsync(string expectation, Func<IReadOnlyList<IWebElement>, bool> holds, Func<IReadOnlyList<IWebElement>, string> describe, bool single = true)
    {
        var deadline = DateTime.UtcNow + AssertTimeout;
        var last = "nothing read yet";
        while (true)
        {
            try
            {
                var elements = _resolve();
                if (single && elements.Count > 1)
                {
                    throw new UiAssertionException($"Ambiguous locator {_description}: matched {elements.Count} elements. Refine the locator or explicitly use First.");
                }
                if (holds(elements))
                {
                    return;
                }
                last = describe(elements);
            }
            catch (WebDriverException ex)
            {
                last = ex.Message;
            }
            if (DateTime.UtcNow >= deadline)
            {
                throw new UiAssertionException($"Expected {_description} {expectation} within {AssertTimeout.TotalSeconds:F0} s; got {last}.");
            }
            await Task.Delay(PollInterval);
        }
    }

    private static string FirstText(IReadOnlyList<IWebElement> elements) =>
        elements.Count > 0 ? $"text \"{Normalize(elements[0].Text)}\"" : "no element";

    private static string Normalize(string? text) => Regex.Replace(text ?? "", @"\s+", " ").Trim();
}
