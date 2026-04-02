// src\Argus.Defender\Monitors\MonitorEvent.cs
namespace Argus.Defender.Monitors;

public enum MonitorEventType { FileCreated, FileModified, FileDeleted, FileRenamed, ProcessStarted, ProcessStopped, RegistryChanged }

public sealed record MonitorEvent
{
    public required MonitorEventType Type { get; init; }
    public required string Path { get; init; }
    public string? OldPath { get; init; }       // For renames
    public string? RegistryKey { get; init; }   // For registry events
    public int? ProcessId { get; init; }        // For process events
    public string? ProcessName { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
