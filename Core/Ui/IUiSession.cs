namespace Core.Ui;

/// <summary>
/// One isolated browser session (cookies, storage, one page), opened per test by <see cref="IUiDriver"/> (R42).
/// Optional features are capabilities: check them with <see cref="TryGet{TCapability}"/>.
/// </summary>
public interface IUiSession : IAsyncDisposable
{
    /// <summary>Opens <paramref name="url"/>, relative to the session's base URL or absolute.</summary>
    Task NavigateAsync(string url);

    /// <summary>The current page's title.</summary>
    Task<string> TitleAsync();

    IElement Find(Locate locate);

    /// <summary>
    /// Returns an optional capability (e.g. <see cref="Capabilities.IScreenshots"/>, <see cref="Capabilities.ITracing"/>)
    /// if this driver supports it.
    /// </summary>
    bool TryGet<TCapability>(out TCapability capability) where TCapability : class;

    /// <summary>
    /// Escape hatch to the driver's own object (e.g. Playwright's <c>IPage</c>) for something the contract doesn't
    /// cover. Keep its use rare and visible: it ties that code to one driver. Throws if the driver has no such object.
    /// </summary>
    T Native<T>() where T : class;
}
