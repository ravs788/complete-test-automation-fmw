using OpenQA.Selenium;

namespace Core.Ui.Selenium;

/// <summary>Maps WebDriver failures to the framework's <see cref="UiException"/>.</summary>
internal static class WebDriverErrors
{
    /// <summary>Failures that may pass if the action is tried again (element not there yet, re-rendered, covered).</summary>
    public static bool IsTransient(Exception ex) =>
        ex is NoSuchElementException or StaleElementReferenceException or ElementNotInteractableException
            or ElementClickInterceptedException or InvalidElementStateException;

    public static void Run(string action, Action body) => Run(action, () => { body(); return true; });

    public static T Run<T>(string action, Func<T> body)
    {
        try
        {
            return body();
        }
        catch (WebDriverException ex)
        {
            throw new UiException($"Could not {action}.{Environment.NewLine}{ex.Message}", ex);
        }
    }
}
