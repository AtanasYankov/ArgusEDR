using Argus.Core;
using Argus.Core.IPC;
using Argus.Core.Models;
using Argus.Watchdog.IPC;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Argus.Watchdog;

public sealed class WatchdogService : BackgroundService
{
    private readonly ILogger<WatchdogService> _log;
    private readonly WatchdogPipeServer _pipe;
    private readonly Dictionary<string, DateTimeOffset> _lastHeartbeat = new();

    public WatchdogService(ILogger<WatchdogService> log, WatchdogPipeServer pipe)
    {
        _log = log;
        _pipe = pipe;
        _pipe.MessageReceived += OnMessageReceived;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _pipe.StartAsync(ct);
        _log.LogInformation("Watchdog started — heartbeat interval {Interval}s, timeout {Timeout}s",
            ArgusConstants.HeartbeatInterval.TotalSeconds,
            ArgusConstants.HeartbeatTimeout.TotalSeconds);

        while (!ct.IsCancellationRequested)
        {
            CheckHeartbeats();
            await Task.Delay(ArgusConstants.HeartbeatInterval, ct);
        }
    }

    private void OnMessageReceived(object? sender, PipeMessage msg)
    {
        if (msg.Version != PipeMessage.CurrentProtocolVersion)
        {
            _log.LogWarning("Rejected message with unknown protocol version {V} from {Sender}",
                msg.Version, msg.SenderModule);
            return;
        }

        switch (msg.Type)
        {
            case PipeMessageType.Heartbeat:
                _lastHeartbeat[msg.SenderModule] = DateTimeOffset.UtcNow;
                break;

            case PipeMessageType.ModuleError:
                var error = msg.GetPayload<ModuleError>();
                if (error?.Severity == ErrorSeverity.Fatal)
                {
                    _log.LogCritical("Fatal error from {Module}: {Message}",
                        error.ModuleId, error.Message);
                    ActivateSafeMode(error.ModuleId);
                }
                else
                {
                    _log.LogWarning("Module error from {Module}: {Message}",
                        error?.ModuleId, error?.Message);
                }
                break;

            case PipeMessageType.ThreatAlert:
                _log.LogWarning("Threat alert from {Sender}: {Payload}",
                    msg.SenderModule, msg.Payload);
                // Phase 5+ will implement quarantine orchestration
                break;

            default:
                _log.LogDebug("Received {Type} from {Sender}",
                    msg.Type, msg.SenderModule);
                break;
        }
    }

    private void CheckHeartbeats()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (module, last) in _lastHeartbeat)
        {
            if (now - last > ArgusConstants.HeartbeatTimeout)
            {
                _log.LogCritical("Module {Module} heartbeat timeout — activating Safe Mode", module);
                ActivateSafeMode(module);
            }
        }
    }

    private void ActivateSafeMode(string offendingModule)
    {
        _log.LogWarning("Safe Mode activated due to {Module}", offendingModule);
        // Phase 8 (Recovery) will implement full safe mode logic:
        // - Write sentinel file to ArgusConstants.SafeModeSentinelPath
        // - Signal Recovery module to verify and restore binaries
        // - Notify GUI via pipe
    }
}
