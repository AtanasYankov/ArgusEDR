using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using Argus.Core;
using Argus.Core.IPC;
using Serilog;

namespace Argus.Watchdog.IPC;

/// <summary>
/// Named pipe server for Watchdog IPC. ACL-restricted to SYSTEM + Administrators + Interactive Users.
/// All messages are HMAC-SHA256 verified. Invalid signatures are logged and rejected.
/// </summary>
public sealed class WatchdogPipeServer : IAsyncDisposable
{
    private readonly byte[] _hmacKey;
    private readonly CancellationTokenSource _cts = new();

    public event EventHandler<PipeMessage>? MessageReceived;

    public WatchdogPipeServer(byte[] hmacKey)
    {
        _hmacKey = hmacKey;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _ = Task.Run(() => AcceptLoopAsync(ct), ct);
        return Task.CompletedTask;
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
        while (!linked.IsCancellationRequested)
        {
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = CreateSecuredPipe();
                await pipe.WaitForConnectionAsync(linked.Token);
                Log.Debug("Pipe client connected");
                _ = Task.Run(() => HandleClientAsync(pipe, linked.Token), linked.Token);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Log.Error(ex, "Pipe accept error");
                pipe?.Dispose();
            }
        }
    }

    private static NamedPipeServerStream CreateSecuredPipe()
    {
        var pipeSecurity = new PipeSecurity();

        // SYSTEM: full control (Watchdog, Defender, Recovery)
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
            PipeAccessRights.FullControl, AccessControlType.Allow));

        // Administrators: read/write (elevated GUI operations)
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            PipeAccessRights.ReadWrite, AccessControlType.Allow));

        // Interactive Users: read/write (standard GUI, Scanner)
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            new SecurityIdentifier(WellKnownSidType.InteractiveSid, null),
            PipeAccessRights.ReadWrite, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            ArgusConstants.PipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,   // Byte mode for length-prefixed framing
            PipeOptions.Asynchronous,
            inBufferSize: 65536, outBufferSize: 65536,
            pipeSecurity);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        try
        {
            while (pipe.IsConnected && !ct.IsCancellationRequested)
            {
                var msg = await PipeMessage.ReadFramedAsync(pipe, _hmacKey, ct);
                if (msg is null) break;

                if (msg.Version != PipeMessage.CurrentProtocolVersion)
                {
                    Log.Warning("Rejected message with unknown protocol v{V} from {Sender}",
                        msg.Version, msg.SenderModule);
                    continue;
                }

                MessageReceived?.Invoke(this, msg);
            }
        }
        catch (CryptographicException ex)
        {
            Log.Warning(ex, "HMAC verification failed — possible spoofing attempt");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Warning(ex, "Pipe client disconnected unexpectedly");
        }
        finally
        {
            pipe.Dispose();
            Log.Debug("Pipe client disconnected");
        }
    }

    public ValueTask DisposeAsync()
    {
        _cts.Cancel();
        return ValueTask.CompletedTask;
    }
}
