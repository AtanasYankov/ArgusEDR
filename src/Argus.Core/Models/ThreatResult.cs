namespace Argus.Core.Models;

public enum ThreatLevel { Clean, Suspicious, Malicious, Unknown }

public sealed class ThreatResult
{
    public string FilePath { get; init; }
    public ThreatLevel Level { get; init; }
    public string? Evidence { get; init; }
    public int Confidence { get; init; }  // 0-100

    public bool IsClean      => Level == ThreatLevel.Clean;
    public bool IsSuspicious => Level == ThreatLevel.Suspicious;
    public bool IsMalicious  => Level == ThreatLevel.Malicious;
    public bool IsUnknown    => Level == ThreatLevel.Unknown;

    private ThreatResult(string filePath, ThreatLevel level, string? evidence, int confidence)
    {
        FilePath = filePath; Level = level; Evidence = evidence; Confidence = confidence;
    }

    public static ThreatResult Clean(string path) =>
        new(path, ThreatLevel.Clean, null, 0);

    public static ThreatResult Suspicious(string path, string evidence, int confidence) =>
        new(path, ThreatLevel.Suspicious, evidence, confidence);

    public static ThreatResult Malicious(string path, string evidence, int confidence) =>
        new(path, ThreatLevel.Malicious, evidence, confidence);

    /// <summary>
    /// FAIL CLOSED: Used when a scanner errors out. Never return Clean on failure.
    /// Unknown results should be treated as potentially dangerous and logged to Watchdog.
    /// </summary>
    public static ThreatResult Unknown(string path, string errorReason) =>
        new(path, ThreatLevel.Unknown, $"ScanError: {errorReason}", 0);
}
