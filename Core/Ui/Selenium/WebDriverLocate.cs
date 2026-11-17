using OpenQA.Selenium;

namespace Core.Ui.Selenium;

/// <summary>Finds elements for a framework <see cref="Locate"/> with WebDriver (CSS, placeholder, ARIA roles via common tags).</summary>
internal static class WebDriverLocate
{
    private static readonly Dictionary<string, string> RoleSelectors = new()
    {
        [nameof(UiRole.Button)] = "button, [role='button'], input[type='button'], input[type='submit'], input[type='reset']",
        [nameof(UiRole.Checkbox)] = "input[type='checkbox'], [role='checkbox']",
        [nameof(UiRole.Link)] = "a[href], [role='link']",
        [nameof(UiRole.Textbox)] = "input:not([type]), input[type='text'], input[type='email'], input[type='password'], input[type='search'], textarea, [role='textbox']",
        [nameof(UiRole.Heading)] = "h1, h2, h3, h4, h5, h6, [role='heading']",
    };

    public static IReadOnlyList<IWebElement> Query(ISearchContext root, Locate locate) => locate.Kind switch
    {
        LocateKind.Placeholder => root.FindElements(By.CssSelector($"[placeholder=\"{locate.Value.Replace("\"", "\\\"")}\"]")),
        LocateKind.Role => root.FindElements(By.CssSelector(RoleSelectors[locate.Value]))
            .Where(e => locate.Name is null || Contains(AccessibleName(e), locate.Name))
            .ToList(),
        _ => root.FindElements(By.CssSelector(locate.Value)),
    };

    public static bool Contains(string? text, string value) =>
        (text ?? "").Contains(value.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>A practical accessible name: aria-label, the element's text or value, or its label's text.</summary>
    private static string AccessibleName(IWebElement element)
    {
        var ariaLabel = element.GetAttribute("aria-label");
        if (!string.IsNullOrWhiteSpace(ariaLabel))
        {
            return ariaLabel;
        }
        var text = element.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            return text;
        }
        var value = element.GetAttribute("value");
        if (!string.IsNullOrWhiteSpace(value) && element.TagName == "input")
        {
            return value;
        }
        var label = element.FindElements(By.XPath("ancestor::label")).FirstOrDefault();
        return label?.Text ?? "";
    }
}
