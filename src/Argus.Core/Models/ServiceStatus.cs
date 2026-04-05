using System.Text.Json.Serialization;

namespace Argus.Core;

public record ServiceStatus(
    bool ServiceRunning,
    bool DefenderActive,
    int ThreatsDetected,
    int FilesScanned,
    int QuarantinedItems,
    DateTime? LastScanTime,
    bool SafeModeActive,
    string? SafeModeReason,
    string? WatchdogStatus,
    string? DefenderStatus);
