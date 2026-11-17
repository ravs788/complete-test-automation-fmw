namespace Core.Resilience;

/// <summary>
/// Waits until a condition holds, for eventually consistent systems (a record that appears after a while, a job that
/// finishes). Use this instead of <c>Thread.Sleep</c> or ad-hoc loops. For UI state, prefer Playwright's
/// auto-retrying <c>Expect(...)</c>.
/// </summary>
public static class Poll
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Re-checks <paramref name="condition"/> every <paramref name="interval"/> (default 500 ms) until it returns true;
    /// throws <see cref="TimeoutException"/> after <paramref name="timeout"/>.
    /// </summary>
    public static async Task UntilAsync(
        Func<Task<bool>> condition, TimeSpan timeout, TimeSpan? interval = null, string description = "condition",
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            if (await condition())
            {
                return;
            }

            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException($"Timed out after {timeout.TotalSeconds:F1} s waiting for {description}.");
            }

            await Task.Delay(interval ?? DefaultInterval, cancellationToken);
        }
    }

    /// <summary>Polls a value until <paramref name="isReady"/> accepts it, and returns that value.</summary>
    public static async Task<T> UntilAsync<T>(
        Func<Task<T>> getValue, Func<T, bool> isReady, TimeSpan timeout, TimeSpan? interval = null,
        string description = "value", CancellationToken cancellationToken = default)
    {
        T value = default!;
        await UntilAsync(async () => isReady(value = await getValue()), timeout, interval, description, cancellationToken);
        return value;
    }
}
