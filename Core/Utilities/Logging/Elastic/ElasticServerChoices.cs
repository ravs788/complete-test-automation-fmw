namespace Core.Utilities
{
    /// <summary>
    /// Mirrors the Java enum (org.ravs788.extensions.report.ElasticServerChoices).
    /// Determines which Elastic connection profile to use when building the REST client.
    /// </summary>
    public enum ElasticServerChoices
    {
        ON_CLOUD,
        ON_LOCALHOST_SECURE,
        ON_LOCALHOST_INSECURE
    }
}
