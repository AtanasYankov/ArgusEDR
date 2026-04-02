namespace Argus.Core.IPC;

/// <summary>
/// Pipe client with exponential backoff retry on connect.
/// All messages are HMAC-signed and length-framed.
/// </summary>
public interface IPipeClient : IDisposable
{
    /// <summary>
    /// Connect to the Watchdog pipe with exponential backoff retry
    /// (5 attempts: 100ms, 200ms, 400ms, 800ms, 1600ms).
    /// </summary>
    Task ConnectAsync(CancellationToken ct);
    Task SendAsync(PipeMessage message, CancellationToken ct);
    Task<PipeMessage?> ReceiveAsync(CancellationToken ct);
}
