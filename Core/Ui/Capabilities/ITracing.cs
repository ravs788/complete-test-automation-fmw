namespace Core.Ui.Capabilities;

/// <summary>Optional UI capability (R42): record a trace (actions, DOM snapshots, network) of the session.</summary>
public interface ITracing
{
    /// <summary>Attachment name for the trace in the report, e.g. <c>Playwright trace</c>.</summary>
    string TraceName { get; }

    /// <summary>Content type to attach the trace with (lets the report open it in the right viewer).</summary>
    string TraceContentType { get; }

    /// <summary>File extension for the trace attachment, e.g. <c>zip</c>.</summary>
    string TraceFileExtension { get; }

    Task StartAsync();

    /// <summary>Stops recording and returns the trace.</summary>
    Task<byte[]> StopAsync();
}
