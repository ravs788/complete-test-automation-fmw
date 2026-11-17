using Core.Configuration;
using Core.Resilience;
using Core.Ui;

namespace Pages.Common;

/// <summary>
/// Base for page objects. Pages work on the framework's UI contract (<see cref="IUiSession"/>, <see cref="IElement"/>,
/// R42), so they run on whichever UI driver <c>providers.json</c> selects.
/// </summary>
public abstract class BasePage(IUiSession session)
{
    protected readonly IUiSession Session = session ?? throw new ArgumentNullException(nameof(session));

    /// <summary>
    /// The attribute the app uses for test ids (default <c>data-testid</c>). Override it in the app's base page, e.g.
    /// SauceDemo uses <c>data-test</c>.
    /// </summary>
    protected virtual string TestIdAttribute => "data-testid";

    /// <summary>How to find an element by test id, for use inside other elements (<see cref="IElement.Find"/>, <see cref="IElement.WithChild"/>).</summary>
    protected Locate ByTestId(string id) => Locate.TestId(TestIdAttribute, id);

    /// <summary>
    /// The element with this test id. Prefer this, or <see cref="Locate.Role"/> / <see cref="Locate.Placeholder"/>,
    /// over CSS classes, which change with styling.
    /// </summary>
    protected IElement TestId(string id) => Session.Find(ByTestId(id));

    /// <summary>
    /// Navigates to <paramref name="url"/>, retrying (per <c>Resilience.Navigation</c> in testsettings.json) when the
    /// browser reports a network error (<c>net::ERR_*</c>, e.g. a connection reset by a busy demo site). Other failures,
    /// and all assertions, are not retried.
    /// </summary>
    protected Task GotoAsync(string url) =>
        RetryPolicies.Navigation(ConfigurationProvider.Current.Resilience.Navigation)
            .ExecuteAsync(_ => Session.NavigateAsync(url));
}
