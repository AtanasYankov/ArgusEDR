namespace Argus.Core.Models;

public sealed record DetectionEvent(
    Guid Id,
    DateTimeOffset Timestamp,
    string Source,          // "Defender", "Scanner"
    ThreatResult Result,
    string? ProcessName,
    int? ProcessId
)
{
    public static DetectionEvent Create(string source, ThreatResult result,
        string? processName = null, int? pid = null)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, source, result, processName, pid);
}
