namespace Argus.Scanner.Intel;

public interface IThreatIntelClient
{
    /// <returns>Malicious score 0-100, or -1 if not found.</returns>
    Task<int> CheckHashAsync(string sha256, CancellationToken ct = default);
    /// <returns>Abuse confidence score 0-100, or -1 if unknown.</returns>
    Task<int> CheckIpAsync(string ip, CancellationToken ct = default);
}
