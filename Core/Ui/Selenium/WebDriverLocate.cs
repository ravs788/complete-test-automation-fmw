using OpenQA.Selenium;

namespace Core.Ui.Selenium;

/// <summary>Resolves framework locators using browser-computed roles and accessible labels.</summary>
internal static class WebDriverLocate
{
    // Candidates only: the browser computes the actual role, including explicit overrides and input types.
    private const string RoleCandidates = "button, input, textarea, select, a, area, h1, h2, h3, h4, h5, h6, [role]";

    public static IReadOnlyList<IWebElement> Query(ISearchContext root, Locate locate) => locate.Kind switch
    {
        LocateKind.Placeholder => root.FindElements(By.CssSelector($"[placeholder=\"{locate.Value.Replace("\"", "\\\"")}\"]")),
        LocateKind.Role => root.FindElements(By.CssSelector(RoleCandidates))
            .Where(e => MatchesRole(e, locate))
            .ToList(),
        _ => root.FindElements(By.CssSelector(locate.Value)),
    };

    public static bool Contains(string? text, string value) =>
        (text ?? "").Contains(value.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesRole(IWebElement element, Locate locate)
    {
        if (element is not WebElement native)
        {
            throw new UiException("Role locators require a WebDriver element with computed accessibility support.");
        }
        if (!string.Equals(native.ComputedAccessibleRole, locate.Value, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        // Opacity and viewport position do not exclude an element from role lookup. Exclude ARIA-hidden and
        // non-rendered ancestry, including closed details, while respecting a child's visibility override.
        var visible = ((IJavaScriptExecutor)native.WrappedDriver).ExecuteScript("""
            const element = arguments[0];
            const visibility = getComputedStyle(element).visibility;
            if (visibility === 'hidden' || visibility === 'collapse') return false;
            for (let node = element; node; node = node.parentElement) {
                if (node.getAttribute('aria-hidden')?.toLowerCase() === 'true' ||
                    getComputedStyle(node).display === 'none') return false;
                if (node.tagName === 'DETAILS' && !node.open) {
                    const summary = node.querySelector(':scope > summary');
                    if (!summary || !summary.contains(element)) return false;
                }
            }
            return true;
            """, native);
        return visible is true && (locate.Name is null || Contains(native.ComputedAccessibleLabel, locate.Name));
    }
}
