namespace Core.Composition;

/// <summary>
/// One adapter project's entry point (R40): registers its provider(s) by name. The test project's composition root
/// (<c>TestRunSetup</c>) passes the plugins it references to <see cref="Framework.Configure(IFrameworkPlugin[])"/>; which registered
/// provider is used is chosen by name in <c>providers.json</c>.
/// </summary>
public interface IFrameworkPlugin
{
    void Register(ProviderRegistry registry);
}
