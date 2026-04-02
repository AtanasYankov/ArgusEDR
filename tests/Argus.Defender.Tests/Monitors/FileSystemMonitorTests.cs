// tests\Argus.Defender.Tests\Monitors\FileSystemMonitorTests.cs
using Argus.Defender.Monitors;
using FluentAssertions;

namespace Argus.Defender.Tests.Monitors;

public class FileSystemMonitorTests
{
    [Fact]
    public async Task Monitor_DetectsNewFile_RaisesEvent()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var tcs = new TaskCompletionSource<string>();

        var monitor = new FileSystemMonitor(new[] { tempDir });
        monitor.FileChanged += (_, path) => tcs.TrySetResult(path);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _ = monitor.StartAsync(cts.Token);

        var newFile = Path.Combine(tempDir, "test.exe");
        await File.WriteAllTextAsync(newFile, "test");

        var detected = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
        detected.Should().Be(newFile);

        monitor.Dispose();
        Directory.Delete(tempDir, true);
    }
}
