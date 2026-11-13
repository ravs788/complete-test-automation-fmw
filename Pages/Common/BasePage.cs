using Microsoft.Playwright;

namespace Pages.Common;

public abstract class BasePage(IPage page)
{
    protected readonly IPage Page = page ?? throw new ArgumentNullException(nameof(page));
}
