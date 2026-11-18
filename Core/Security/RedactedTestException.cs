namespace Core.Security;

/// <summary>A sanitized failure for the test host; retains safe original type/stack text but no raw inner exception.</summary>
public sealed class RedactedTestException(Exception original) : Exception(DiagnosticRedactor.Redact(original.ToString()))
{
    public static void Run(Action body)
    {
        try { body(); }
        catch (Exception ex)
        {
            if (DiagnosticRedactor.Redact(ex.ToString()) != ex.ToString()) throw new RedactedTestException(ex);
            throw;
        }
    }

    public static async Task RunAsync(Func<Task> body)
    {
        try { await body(); }
        catch (Exception ex)
        {
            if (DiagnosticRedactor.Redact(ex.ToString()) != ex.ToString()) throw new RedactedTestException(ex);
            throw;
        }
    }
}
