using System.Reflection;
using Core.Api;
using Core.Http.Playwright;
using Core.Logging.Elastic;
using Core.Messaging.Kafka;
using Core.Messaging.RabbitMq;
using Core.Mobile.Driver;
using Core.Reporting.Allure;
using Core.Resilience.Polly;
using Core.Security.AzureKeyVault;
using Core.Ui.Playwright;
using Core.Ui.Selenium;

namespace Tests.Unit.Architecture;

/// <summary>
/// Plug-and-play guardrails (R39–R46): vendor packages live in adapter projects, not in Core, and an adapter
/// depends on Core only, never on another adapter or on app or test code. Extended as each roadmap phase lands.
/// </summary>
public class AdapterBoundaryTests
{
    private static readonly Assembly Core = typeof(ApiClient).Assembly;

    public static TheoryData<string> Adapters => new()
    {
        typeof(ElasticLoggingPlugin).Assembly.GetName().Name!,
        typeof(PlaywrightHttpPlugin).Assembly.GetName().Name!,
        typeof(AllureReportingPlugin).Assembly.GetName().Name!,
        typeof(PollyResiliencePlugin).Assembly.GetName().Name!,
        typeof(PlaywrightUiPlugin).Assembly.GetName().Name!,
        typeof(SeleniumUiPlugin).Assembly.GetName().Name!,
        typeof(AzureKeyVaultPlugin).Assembly.GetName().Name!,
        typeof(RabbitMqListener).Assembly.GetName().Name!,
        typeof(KafkaListener).Assembly.GetName().Name!,
        typeof(MobileDriverFactory).Assembly.GetName().Name!,
    };

    [Theory]
    [InlineData("Elastic.")]
    [InlineData("Allure")]
    [InlineData("Polly")]
    [InlineData("Microsoft.Playwright")]
    [InlineData("Azure.")]
    [InlineData("RabbitMQ.")]
    [InlineData("Confluent.")]
    public void Core_Should_NotReference_VendorsThatLiveInAdapters(string vendorPrefix)
    {
        Assert.DoesNotContain(Core.GetReferencedAssemblies(), a => a.Name!.StartsWith(vendorPrefix, StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Adapters))]
    public void Adapter_Should_DependOn_CoreOnly_AmongFrameworkProjects(string adapter)
    {
        var frameworkReferences = Assembly.Load(adapter).GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => name.StartsWith("Core", StringComparison.Ordinal) || name is "Domain" or "Pages" || name.StartsWith("Tests", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();

        // The one deliberate exception: Appium speaks W3C WebDriver, so the Appium adapter builds on the WebDriver
        // session of the Selenium adapter (R47).
        string[] expected = adapter == "Core.Mobile" ? ["Core", "Core.Ui.Selenium"] : ["Core"];
        Assert.Equal(expected, frameworkReferences);
    }

    [Fact]
    public void Pages_Should_UseTheUiContract_NotPlaywright()
    {
        var pages = typeof(Pages.Common.BasePage).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();

        Assert.DoesNotContain("Microsoft.Playwright", pages);
        Assert.Contains("Core", pages);
    }
}
