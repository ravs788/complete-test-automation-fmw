using System.Reflection;
using Core.Api;
using Core.Logging.Elastic;
using Core.Messaging.Kafka;
using Core.Messaging.RabbitMq;
using Core.Mobile.Driver;
using Core.Security.AzureKeyVault;

namespace Tests.Unit.Architecture;

/// <summary>
/// Plug-and-play guardrails (R40, R44, R46): vendor packages live in adapter projects, not in Core, and an adapter
/// depends on Core only, never on another adapter or on app or test code. Extended as each roadmap phase lands.
/// </summary>
public class AdapterBoundaryTests
{
    private static readonly Assembly Core = typeof(ApiClient).Assembly;

    public static TheoryData<string> Adapters => new()
    {
        typeof(ElasticLoggingPlugin).Assembly.GetName().Name!,
        typeof(AzureKeyVaultPlugin).Assembly.GetName().Name!,
        typeof(RabbitMqListener).Assembly.GetName().Name!,
        typeof(KafkaListener).Assembly.GetName().Name!,
        typeof(MobileDriverFactory).Assembly.GetName().Name!,
    };

    [Theory]
    [InlineData("Elastic.")]
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
