namespace Core.Ui;

/// <summary>
/// Web-first assertions (R42): each one keeps re-checking until it holds or the driver's assertion timeout passes
/// (Playwright: 5 s), then throws <see cref="UiAssertionException"/>. Never wrap them in a retry.
/// Scalar assertions require a single match; count and list-text assertions accept collections.
/// </summary>
public interface IElementAssertions
{
    Task ToBeVisibleAsync();

    /// <summary>The element's whole text equals <paramref name="text"/> (whitespace normalised).</summary>
    Task ToHaveTextAsync(string text);

    /// <summary>The elements' texts equal <paramref name="texts"/>, in order.</summary>
    Task ToHaveTextAsync(IReadOnlyList<string> texts);

    Task ToContainTextAsync(string text, bool ignoreCase = false);

    Task ToHaveCountAsync(int count);

    /// <summary>The element's class list contains <paramref name="className"/>.</summary>
    Task ToHaveClassAsync(string className);
}
