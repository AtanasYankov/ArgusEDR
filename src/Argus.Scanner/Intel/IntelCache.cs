using System.Collections.Concurrent;

namespace Argus.Scanner.Intel;

public sealed class IntelCache
{
    private sealed record Entry(int Score, DateTimeOffset Expiry);
    private readonly ConcurrentDictionary<string, Entry> _hashes = new();
    private readonly ConcurrentDictionary<string, Entry> _ips = new();

    public void SetHash(string sha256, int score, TimeSpan ttl)
        => _hashes[sha256] = new Entry(score, DateTimeOffset.UtcNow.Add(ttl));

    public bool TryGetHash(string sha256, out int score)
    {
        if (_hashes.TryGetValue(sha256, out var e) && e.Expiry > DateTimeOffset.UtcNow)
        { score = e.Score; return true; }
        score = 0; return false;
    }

    public void SetIp(string ip, int abuseScore, TimeSpan ttl)
        => _ips[ip] = new Entry(abuseScore, DateTimeOffset.UtcNow.Add(ttl));

    public bool TryGetIp(string ip, out int score)
    {
        if (_ips.TryGetValue(ip, out var e) && e.Expiry > DateTimeOffset.UtcNow)
        { score = e.Score; return true; }
        score = 0; return false;
    }
}
