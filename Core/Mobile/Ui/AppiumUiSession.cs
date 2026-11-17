using Core.Mobile.Interactions;
using Core.Ui;
using Core.Ui.Capabilities;
using Core.Ui.Selenium;
using OpenQA.Selenium.Appium;

namespace Core.Mobile.Ui;

/// <summary>
/// <see cref="IUiSession"/> on an Appium session: the WebDriver session from <c>Core.Ui.Selenium</c> (Appium speaks
/// W3C WebDriver) plus the <see cref="IMobileGestures"/> capability.
/// </summary>
public sealed class AppiumUiSession(AppiumDriver driver, UiSessionOptions options) : WebDriverUiSession(driver, options), IMobileGestures
{
    public Task SwipeAsync(SwipeDirection direction, double screenPercent = 0.75)
    {
        GestureHelper.SwipeOnScreen(driver, direction.ToString(), screenPercent);
        return Task.CompletedTask;
    }

    public Task ScrollAsync(SwipeDirection direction, double screenPercent = 0.75)
    {
        GestureHelper.ScrollOnScreen(driver, direction.ToString(), screenPercent);
        return Task.CompletedTask;
    }

    public async Task<bool> ScrollUntilVisibleAsync(IElement element, SwipeDirection direction = SwipeDirection.Down, int maxScrolls = 6)
    {
        for (var i = 0; i < maxScrolls; i++)
        {
            if (await element.IsVisibleAsync())
            {
                return true;
            }
            await ScrollAsync(direction);
        }
        return await element.IsVisibleAsync();
    }
}
