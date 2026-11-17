namespace Core.Reporting;

/// <summary>Tags for a test class in the report (R48), e.g. <c>[ReportTags("ui", "saucedemo", "checkout")]</c>.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ReportTagsAttribute(params string[] tags) : Attribute
{
    public IReadOnlyList<string> Tags { get; } = tags;
}
