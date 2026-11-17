namespace Core.Composition;

/// <summary>Names of the provider kinds selected in <c>providers.json</c> (also used in startup and error messages).</summary>
public static class ProviderKinds
{
    public const string Logging = "Logging";
    public const string ResultsPublisher = "ResultsPublisher";
    public const string Secrets = "Secrets";
    public const string Reporting = "Reporting";
    public const string Http = "Http";
    public const string Resilience = "Resilience";
    public const string Ui = "Ui";
}
