using System.Reflection;

namespace Tests.Unit.Architecture;

/// <summary>
/// xUnit is the host, kept to a thin layer (R48): scenario test classes use only xUnit's attributes
/// (<c>[Fact]</c>, <c>[Theory]</c>, <c>[Trait]</c>) and <c>Assert</c>; xUnit's lifecycle interfaces live in fixtures
/// and base classes; no test project references a vendor library besides xUnit. Checks read metadata only, so no
/// test-project code (e.g. its composition root) runs here.
/// </summary>
public class TestHostBoundaryTests
{
    private static readonly string[] TestAssemblies = ["Tests", "Tests.UI.Mobile"];

    private static readonly string[] VendorPrefixes =
        ["Microsoft.Playwright", "OpenQA.", "Appium", "WebDriver", "Allure", "Elastic.", "Polly", "Azure.", "RabbitMQ", "Confluent."];

    public static TheoryData<string> Assemblies => new(TestAssemblies);

    private static Assembly Load(string name) => Assembly.Load(new AssemblyName(name));

    private static bool IsTestClass(Type type) =>
        type.GetMethods().Any(m => m.GetCustomAttributesData().Any(a => a.AttributeType.FullName is "Xunit.FactAttribute" or "Xunit.TheoryAttribute"));

    private static IEnumerable<Type> DeclaredInterfaces(Type type) =>
        type.GetInterfaces().Except(type.BaseType?.GetInterfaces() ?? []);

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void TestClasses_Should_UseNoXunitInterfaces_AndNoReportingLibraryTypes(string assembly)
    {
        var offenders = Load(assembly).GetTypes()
            .Where(IsTestClass)
            .SelectMany(t => DeclaredInterfaces(t).Where(i => i.Namespace?.StartsWith("Xunit", StringComparison.Ordinal) == true).Select(i => $"{t.Name}: {i.Name}")
                .Concat(t.GetCustomAttributesData().Where(a => a.AttributeType.Namespace?.StartsWith("Allure", StringComparison.Ordinal) == true).Select(a => $"{t.Name}: [{a.AttributeType.Name}]")))
            .ToList();

        Assert.Empty(offenders);
    }

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void XunitLifecycleInterfaces_Should_OnlyBeImplemented_InFixturesAndBaseClasses(string assembly)
    {
        var offenders = Load(assembly).GetTypes()
            .Where(t => DeclaredInterfaces(t).Any(i => i.Namespace?.StartsWith("Xunit", StringComparison.Ordinal) == true))
            .Where(t => t.Namespace is null || !(t.Namespace.EndsWith(".Fixtures", StringComparison.Ordinal) || t.Namespace.EndsWith(".Infrastructure", StringComparison.Ordinal)))
            .Select(t => t.FullName)
            .ToList();

        Assert.Empty(offenders);
    }

    [Theory]
    [InlineData("Tests")]
    [InlineData("Tests.UI.Mobile")]
    [InlineData("Domain")]
    public void Assembly_Should_ReferenceNoVendorLibrary_ButTheTestHost(string assembly)
    {
        var vendors = Load(assembly).GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => VendorPrefixes.Any(p => name.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(vendors);
    }
}
