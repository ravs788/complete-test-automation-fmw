namespace Automation.Pages.Common;

using Microsoft.Playwright;
using System;

public abstract class BasePage(IPage page)
{
    protected readonly IPage Page = page ?? throw new ArgumentNullException(nameof(page));
}
