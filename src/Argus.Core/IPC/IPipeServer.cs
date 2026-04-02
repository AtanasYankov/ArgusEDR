namespace Argus.Core.IPC;

public interface IPipeServer : IAsyncDisposable
{
    Task StartAsync(CancellationToken ct);
    event EventHandler<PipeMessage> MessageReceived;
    Task SendAsync(PipeMessage message, CancellationToken ct);
}
