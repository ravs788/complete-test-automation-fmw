using Microsoft.Playwright;
using Pages.Common;

namespace Pages.SauceDemo;

/// <summary>Base for SauceDemo pages: the app marks its elements with <c>data-test</c>.</summary>
public abstract class SauceDemoPage(IPage page) : BasePage(page)
{
    protected override string TestIdAttribute => "data-test";

    /// <summary>The page heading ("Products", "Your Cart", "Checkout: Overview", …).</summary>
    public ILocator Title => TestId("title");
}
