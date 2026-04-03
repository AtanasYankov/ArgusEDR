using System.Text.Json;

namespace Argus.Scanner.Intel;

public sealed class ThreatIntelClient : IThreatIntelClient
{
    private readonly HttpClient _http;
    private readonly IntelCache _cache;
    private readonly string _vtApiKey;
    private readonly string _abuseIpDbKey;

    public ThreatIntelClient(HttpClient http, IntelCache cache,
        string vtApiKey, string abuseIpDbKey)
    {
        _http = http; _cache = cache;
        _vtApiKey = vtApiKey; _abuseIpDbKey = abuseIpDbKey;
    }

    public async Task<int> CheckHashAsync(string sha256, CancellationToken ct = default)
    {
        if (_cache.TryGetHash(sha256, out var cached)) return cached;

        using var req = new HttpRequestMessage(HttpMethod.Get,
            $"https://www.virustotal.com/api/v3/files/{sha256}");
        req.Headers.Add("x-apikey", _vtApiKey);

        using var resp = await _http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return -1;

        using var doc = await JsonDocument.ParseAsync(
            await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        var stats = doc.RootElement
            .GetProperty("data").GetProperty("attributes")
            .GetProperty("last_analysis_stats");

        int malicious = stats.GetProperty("malicious").GetInt32();
        int total = stats.EnumerateObject().Sum(p => p.Value.GetInt32());
        int score = total == 0 ? 0 : (int)((double)malicious / total * 100);

        _cache.SetHash(sha256, score, TimeSpan.FromHours(24));
        return score;
    }

    public async Task<int> CheckIpAsync(string ip, CancellationToken ct = default)
    {
        if (_cache.TryGetIp(ip, out var cached)) return cached;

        using var req = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.abuseipdb.com/api/v2/check?ipAddress={Uri.EscapeDataString(ip)}");
        req.Headers.Add("Key", _abuseIpDbKey);
        req.Headers.Add("Accept", "application/json");

        using var resp = await _http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return -1;

        using var doc = await JsonDocument.ParseAsync(
            await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        int score = doc.RootElement
            .GetProperty("data").GetProperty("abuseConfidenceScore").GetInt32();

        _cache.SetIp(ip, score, TimeSpan.FromHours(1));
        return score;
    }
}
