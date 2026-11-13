namespace Core.Logging.Elastic;

/// <summary>Outcome of <see cref="ElasticConnectivity.Check"/>: whether the server can be written to, and why not.</summary>
public sealed record ElasticProbeResult(bool IsUsable, string? Reason)
{
    public static ElasticProbeResult Usable { get; } = new(true, null);

    public static ElasticProbeResult NotUsable(string reason) => new(false, reason);
}
