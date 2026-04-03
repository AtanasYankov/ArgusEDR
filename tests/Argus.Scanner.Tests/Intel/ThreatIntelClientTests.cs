using Argus.Scanner.Intel;
using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http;

namespace Argus.Scanner.Tests.Intel;

public class IntelCacheTests
{
    [Fact]
    public void Cache_StoresAndRetrievesHashResult()
    {
        var cache = new IntelCache();
        cache.SetHash("abc123", 90, TimeSpan.FromHours(24));

        var result = cache.TryGetHash("abc123", out var score);

        result.Should().BeTrue();
        score.Should().Be(90);
    }

    [Fact]
    public void Cache_ExpiredEntry_ReturnsNotFound()
    {
        var cache = new IntelCache();
        cache.SetHash("expired", 50, TimeSpan.FromMilliseconds(1));
        Thread.Sleep(10);

        var result = cache.TryGetHash("expired", out _);

        result.Should().BeFalse();
    }
}
