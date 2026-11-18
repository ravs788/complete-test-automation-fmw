using System.Reflection;
using Core.Api;
using Core.Mobile.Driver;

namespace Tests.Unit.Architecture;

/// <summary>
/// Core is the domain-agnostic engine (R34): it must not depend on app-specific code, page objects or tests,
/// so it can be shared by other teams (e.g. as an internal NuGet package).
/// </summary>
public class CoreBoundaryTests
{
    private static readonly Assembly Core = typeof(ApiClient).Assembly;

    private static readonly string[] AppNames = ["RestfulBooker", "SauceDemo", "Todo"];

    [Theory]
    [InlineData("Domain")]
    [InlineData("Pages")]
    [InlineData("Tests")]
    [InlineData("Tests.UI.Mobile")]
    public void Core_Should_NotReference_HigherLayers(string assemblyName)
    {
        // Arrange: use Core and the prohibited assembly name supplied by the theory.
        // Act & Assert
        Assert.DoesNotContain(Core.GetReferencedAssemblies(), a => a.Name == assemblyName);
    }

    [Fact]
    public void Core_Should_NotContain_AppSpecificNamespaces()
    {
        // Arrange: use Core and the prohibited application namespace names.
        // Act
        var appSpecific = Core.GetTypes()
            .Select(t => t.Namespace ?? "")
            .Where(ns => AppNames.Any(app => ns.Split('.').Contains(app)))
            .Distinct()
            .ToList();

        // Assert
        Assert.Empty(appSpecific);
    }

    [Theory]
    [InlineData("Domain")]
    [InlineData("Pages")]
    [InlineData("Tests")]
    [InlineData("Tests.UI.Mobile")]
    public void CoreMobile_Should_NotReference_HigherLayers(string assemblyName)
    {
        // Arrange: use Core.Mobile and the prohibited assembly name supplied by the theory.
        // Act & Assert
        Assert.DoesNotContain(typeof(MobileDriverFactory).Assembly.GetReferencedAssemblies(), a => a.Name == assemblyName);
    }

    [Fact]
    public void Core_Should_NotDependOn_Appium()
    {
        // Arrange: inspect Core for Appium and Selenium references.
        // Act & Assert
        Assert.DoesNotContain(Core.GetReferencedAssemblies(), a => a.Name is "Appium.Net" or "WebDriver");
    }
}
