namespace Core.Resilience;

/// <summary>The one retry log line format every <see cref="IRetryPolicyFactory"/> uses.</summary>
public static class RetryLog
{
    /// <param name="attempt">1 for the first retry.</param>
    /// <param name="maxRetries">The configured number of retries.</param>
    /// <param name="delay">Wait before this retry.</param>
    /// <param name="reason">Exception message or result description; only its first line is logged.</param>
    public static string Line(int attempt, int maxRetries, TimeSpan delay, string? reason) =>
        $"[Retry] attempt {attempt} of {maxRetries} after {delay.TotalMilliseconds:F0} ms: {(reason ?? "").Split('\n')[0].Trim()}";
}
