namespace Tests.Unit;

/// <summary>
/// A [Fact] that only runs when an environment variable is set (e.g. a broker address for an integration test);
/// otherwise it is reported as skipped with the variable named.
/// </summary>
public sealed class RequiresEnvironmentFactAttribute : FactAttribute
{
    public RequiresEnvironmentFactAttribute(string variable)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable)))
        {
            Skip = $"Set {variable} to run this integration test.";
        }
    }
}
