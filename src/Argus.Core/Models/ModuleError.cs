namespace Argus.Core.Models;

/// <summary>
/// Unified error model for cross-module error propagation via IPC.
/// All modules use this to report errors to Watchdog, which can then decide
/// whether to escalate (Safe Mode) or log (transient failure).
/// </summary>
public enum ErrorSeverity { Transient, Permanent, Fatal }

public sealed record ModuleError(
    string ModuleId,          // "Defender", "Scanner", "Engine", "Recovery"
    ErrorSeverity Severity,
    string Message,
    string? ExceptionType = null,
    DateTimeOffset Timestamp = default
)
{
    public static ModuleError Transient(string module, string message, Exception? ex = null) =>
        new(module, ErrorSeverity.Transient, message, ex?.GetType().Name,
            Timestamp: DateTimeOffset.UtcNow);

    public static ModuleError Fatal(string module, string message, Exception? ex = null) =>
        new(module, ErrorSeverity.Fatal, message, ex?.GetType().Name,
            Timestamp: DateTimeOffset.UtcNow);
}
