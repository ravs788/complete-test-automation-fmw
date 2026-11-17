namespace Core.Ui;

/// <summary>
/// A lazy description of one or more elements (R42): nothing is looked up until an action or assertion runs, and
/// every action waits for the element (auto-wait or polling, depending on the driver). Driver failures surface as
/// <see cref="UiException"/>.
/// </summary>
public interface IElement
{
    /// <summary>Human-readable description, used in failure messages.</summary>
    string Description { get; }

    /// <summary>Elements matching <paramref name="locate"/> inside this one.</summary>
    IElement Find(Locate locate);

    /// <summary>Those of these elements that contain a <paramref name="child"/> whose text contains <paramref name="text"/> (case-insensitive).</summary>
    IElement WithChild(Locate child, string text);

    /// <summary>The first of these elements.</summary>
    IElement First { get; }

    Task ClickAsync();

    /// <summary>Replaces the element's value with <paramref name="text"/>.</summary>
    Task FillAsync(string text);

    /// <summary>Presses a key, e.g. <c>Enter</c>.</summary>
    Task PressAsync(string key);

    /// <summary>Ticks a checkbox (no-op if it is already ticked).</summary>
    Task CheckAsync();

    Task<string> TextAsync();

    Task<int> CountAsync();

    Task<bool> IsVisibleAsync();

    /// <summary>Retrying assertions on this element; prefer <see cref="Ui.Expect"/>.</summary>
    IElementAssertions Expect();
}
