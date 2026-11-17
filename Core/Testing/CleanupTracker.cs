namespace Core.Testing;

/// <summary>
/// Records what a test created (bookings, database rows, files, queue subscriptions…) and removes it afterwards, so
/// shared environments aren't left with test data (R5, R37). Cleanups run in reverse order of registration; a failing
/// cleanup is reported through the log callback and never fails the test or stops the remaining cleanups.
/// </summary>
public sealed class CleanupTracker
{
    private readonly Stack<(string Description, Func<Task> Action)> _actions = new();
    private readonly object _gate = new();

    public int Count
    {
        get { lock (_gate) { return _actions.Count; } }
    }

    /// <summary>Registers <paramref name="cleanup"/>, described by <paramref name="description"/> in failure messages.</summary>
    public void Add(string description, Func<Task> cleanup)
    {
        lock (_gate)
        {
            _actions.Push((description, cleanup));
        }
    }

    /// <summary>Runs every registered cleanup (last registered first) and empties the tracker.</summary>
    public async Task RunAsync(Action<string>? onError = null)
    {
        while (true)
        {
            (string Description, Func<Task> Action) next;
            lock (_gate)
            {
                if (!_actions.TryPop(out next))
                {
                    return;
                }
            }

            try
            {
                await next.Action();
            }
            catch (Exception ex)
            {
                onError?.Invoke($"[Cleanup] {next.Description} failed: {ex.Message}");
            }
        }
    }
}
