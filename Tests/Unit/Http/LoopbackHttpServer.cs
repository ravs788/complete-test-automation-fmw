using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Tests.Unit.Http;

/// <summary>
/// A tiny HTTP server on 127.0.0.1 for transport contract tests. <c>/echo…</c> answers 200 with what it received
/// (method, path, body, content type, cookie, accept) and an <c>X-Test</c> header; <c>/missing</c> answers 404.
/// </summary>
public sealed class LoopbackHttpServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _stop = new();

    public LoopbackHttpServer()
    {
        Port = FreePort();
        _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
        _listener.Start();
        _ = Task.Run(ServeAsync);
    }

    public string? FixedResponseJson { get; init; }
    public ConcurrentQueue<string> RequestBodies { get; } = new();

    public ConcurrentDictionary<string, (int Status, string Location)> Redirects { get; } = new();

    public ConcurrentQueue<string> RequestPaths { get; } = new();
    public TaskCompletionSource DelayedRequestReceived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void ReleaseDelayedResponse() => _release.TrySetResult();

    public int Port { get; }

    public string BaseUrl => $"http://127.0.0.1:{Port}/";

    /// <summary>A port nothing listens on, for "no response" tests.</summary>
    public static int FreePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    private async Task ServeAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (_stop.IsCancellationRequested)
            {
                return;
            }
            catch (HttpListenerException)
            {
                return;
            }

            _ = RespondAsync(context);
        }
    }

    private async Task RespondAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;
            RequestPaths.Enqueue(request.Url!.AbsolutePath);
            if (request.Url.AbsolutePath.EndsWith("/delay", StringComparison.Ordinal))
            {
                DelayedRequestReceived.TrySetResult();
                await _release.Task.WaitAsync(_stop.Token);
            }
            using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            RequestBodies.Enqueue(body);
            byte[] payload;
            if (Redirects.TryGetValue(request.Url!.AbsolutePath, out var redirect))
            {
                response.StatusCode = redirect.Status;
                response.Headers["Location"] = redirect.Location;
                payload = [];
            }
            else if (FixedResponseJson is not null)
            {
                response.StatusCode = 200;
                response.ContentType = "application/json";
                response.Headers["X-Elastic-Product"] = "Elasticsearch";
                payload = Encoding.UTF8.GetBytes(FixedResponseJson);
            }
            else if (request.Url!.AbsolutePath.EndsWith("/missing", StringComparison.Ordinal))
            {
                response.StatusCode = 404;
                payload = Encoding.UTF8.GetBytes("missing");
            }
            else
            {
                response.StatusCode = 200;
                response.ContentType = "application/json";
                response.Headers["X-Test"] = "yes";
                payload = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string?>
                {
                    ["method"] = request.HttpMethod,
                    ["path"] = request.Url.AbsolutePath,
                    ["body"] = body,
                    ["contentType"] = request.ContentType,
                    ["cookie"] = request.Headers["Cookie"],
                    ["accept"] = request.Headers["Accept"],
                });
            }
            response.ContentLength64 = payload.Length;
            await response.OutputStream.WriteAsync(payload);
            response.Close();
        }
        catch (Exception ex) when (_stop.IsCancellationRequested || ex is HttpListenerException or IOException or ObjectDisposedException)
        {
            // Cancellation tests deliberately disconnect clients or stop a pending server response.
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Close();
        _stop.Dispose();
    }
}
