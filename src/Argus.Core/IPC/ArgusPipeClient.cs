using System.IO.Pipes;
using Serilog;

namespace Argus.Core.IPC;

public sealed class ArgusPipeClient : IPipeClient
{
    private const string PipeName = "ArgusEDR";
    private const int MaxRetries = 5;
    private NamedPipeClientStream? _pipe;
    private readonly byte[] _hmacKey;
    private readonly string _moduleName;

    public ArgusPipeClient(byte[] hmacKey, string moduleName)
    {
        _hmacKey = hmacKey;
        _moduleName = moduleName;
    }

    public async Task ConnectAsync(CancellationToken ct)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                _pipe = new NamedPipeClientStream(".", PipeName,
                    PipeDirection.InOut, PipeOptions.Asynchronous);
                await _pipe.ConnectAsync(TimeSpan.FromSeconds(5), ct);
                Log.Information("{Module} connected to Watchdog pipe (attempt {N})",
                    _moduleName, attempt + 1);
                return;
            }
            catch (TimeoutException) when (attempt < MaxRetries - 1)
            {
                var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt));
                Log.Warning("{Module} pipe attempt {N} failed, retrying in {Delay}ms",
                    _moduleName, attempt + 1, delay.TotalMilliseconds);
                await Task.Delay(delay, ct);
            }
        }
        throw new TimeoutException(
            $"{_moduleName} failed to connect to Watchdog pipe after {MaxRetries} attempts");
    }

    public async Task SendAsync(PipeMessage message, CancellationToken ct)
    {
        if (_pipe is null || !_pipe.IsConnected)
            throw new InvalidOperationException("Not connected to pipe");
        var frame = message.ToFramedBytes(_hmacKey);
        await _pipe.WriteAsync(frame, ct);
        await _pipe.FlushAsync(ct);
    }

    public async Task<PipeMessage?> ReceiveAsync(CancellationToken ct)
    {
        if (_pipe is null || !_pipe.IsConnected) return null;
        return await PipeMessage.ReadFramedAsync(_pipe, _hmacKey, ct);
    }

    public void Dispose() => _pipe?.Dispose();
}
