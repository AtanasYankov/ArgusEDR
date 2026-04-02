// tests\Argus.Defender.Tests\Monitors\EventPipelineTests.cs
using Argus.Defender.Monitors;
using FluentAssertions;

namespace Argus.Defender.Tests.Monitors;

public class EventPipelineTests
{
    [Fact]
    public void TryPublish_SingleEvent_ReturnsTrue()
    {
        using var pipeline = new EventPipeline(capacity: 100);
        var evt = new MonitorEvent { Type = MonitorEventType.FileCreated, Path = @"C:\test.exe" };

        pipeline.TryPublish(evt).Should().BeTrue();
        pipeline.Received.Should().Be(1);
        pipeline.Processed.Should().Be(1);
    }

    [Fact]
    public void TryPublish_DuplicatePath_IsDeduplicated()
    {
        using var pipeline = new EventPipeline(capacity: 100, dedupeWindow: TimeSpan.FromSeconds(10));
        var evt = new MonitorEvent { Type = MonitorEventType.FileModified, Path = @"C:\test.exe" };

        pipeline.TryPublish(evt).Should().BeTrue();
        pipeline.TryPublish(evt).Should().BeFalse(); // duplicate
        pipeline.Received.Should().Be(2);
        pipeline.Dropped.Should().Be(1);
    }

    [Fact]
    public void TryPublish_BoundedCapacity_DropsOldest()
    {
        using var pipeline = new EventPipeline(capacity: 2);

        for (int i = 0; i < 5; i++)
        {
            pipeline.TryPublish(new MonitorEvent
            {
                Type = MonitorEventType.FileCreated,
                Path = $@"C:\file{i}.exe"
            });
        }

        // All 5 should be received, but channel only holds 2
        pipeline.Received.Should().Be(5);
    }
}
