namespace Core.Reporting;

/// <summary>
/// Groups a test class in the report (R48), e.g. <c>[ReportSuite("UI Web", "SauceDemo", "Checkout")]</c>: layer, app,
/// feature. <see cref="Testing.TestRunner"/> passes it to the selected <see cref="ITestReporter"/> (Allure: the
/// parentSuite / suite / subSuite labels), so test classes don't depend on a reporting library.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ReportSuiteAttribute(string parentSuite, string suite, string subSuite) : Attribute
{
    public string ParentSuite { get; } = parentSuite;

    public string Suite { get; } = suite;

    public string SubSuite { get; } = subSuite;
}
