namespace Core.Ui.Capabilities;

/// <summary>Optional UI capability (R47): touch gestures on a mobile device (Appium).</summary>
public interface IMobileGestures
{
    /// <summary>Swipes across the screen (e.g. <see cref="SwipeDirection.Left"/> to page a carousel).</summary>
    Task SwipeAsync(SwipeDirection direction, double screenPercent = 0.75);

    /// <summary>Scrolls the screen in <paramref name="direction"/> (content moves the opposite way).</summary>
    Task ScrollAsync(SwipeDirection direction, double screenPercent = 0.75);

    /// <summary>Scrolls until <paramref name="element"/> is visible; false if it isn't after <paramref name="maxScrolls"/>.</summary>
    Task<bool> ScrollUntilVisibleAsync(IElement element, SwipeDirection direction = SwipeDirection.Down, int maxScrolls = 6);
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right,
}
