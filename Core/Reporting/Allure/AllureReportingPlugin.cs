using Core.Composition;

namespace Core.Reporting.Allure;

/// <summary>Registers the <c>allure</c> reporter (select it with <c>"Reporting": "allure"</c> in <c>providers.json</c>).</summary>
public sealed class AllureReportingPlugin : IFrameworkPlugin
{
    public void Register(ProviderRegistry registry) =>
        registry.Add<ITestReporter>("allure", () => new AllureTestReporter());
}
