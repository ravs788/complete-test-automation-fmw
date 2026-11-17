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

namespace Tests.Unit.Architecture;

/// <summary>
/// Plug-and-play guardrails (R39, R40, R41, R44, R45, R46): vendor packages live in adapter projects, not in Core, and an adapter
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
        typeof(AzureKeyVaultPlugin).Assembly.GetName().Name!,
        typeof(RabbitMqListener).Assembly.GetName().Name!,
        typeof(KafkaListener).Assembly.GetName().Name!,
        typeof(MobileDriverFactory).Assembly.GetName().Name!,
    };

    [Theory]
    [InlineData("Elastic.")]
    [InlineData("Allure")]
    [InlineData("Polly")]
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
            .ToList();

        Assert.Equal(["Core"], frameworkReferences);
    }
}
