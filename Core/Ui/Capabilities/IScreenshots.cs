namespace Core.Ui.Capabilities;

/// <summary>Optional UI capability (R42): screenshots of the current page.</summary>
public interface IScreenshots
{
    /// <summary>PNG bytes of the page (the whole scrollable page when <paramref name="fullPage"/> is true).</summary>
    Task<byte[]> CaptureAsync(bool fullPage = true);
}
