// src\Argus.Defender\Monitors\EventPipeline.cs
using System.Threading.Channels;
using Serilog;

namespace Argus.Defender.Monitors;

public sealed class EventPipeline : IDisposable
{
    private readonly Channel<MonitorEvent> _channel;
    private readonly HashSet<string> _recentPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _dedupeWindow;
    private long _received;
    private long _dropped;
    private long _processed;

    public EventPipeline(int capacity = 50_000, TimeSpan? dedupeWindow = null)
    {
        _dedupeWindow = dedupeWindow ?? TimeSpan.FromSeconds(2);
        _channel = Channel.CreateBounded<MonitorEvent>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false
        });
    }

    public ChannelReader<MonitorEvent> Reader => _channel.Reader;
    public long Received => Interlocked.Read(ref _received);
    public long Dropped => Interlocked.Read(ref _dropped);
    public long Processed => Interlocked.Read(ref _processed);

    public bool TryPublish(MonitorEvent evt)
    {
        Interlocked.Increment(ref _received);

        // Dedup: skip if same path seen within window
        lock (_recentPaths)
        {
            if (_recentPaths.Contains(evt.Path))
            {
                Interlocked.Increment(ref _dropped);
                return false;
            }
            _recentPaths.Add(evt.Path);
        }

        // Schedule dedup expiry
        _ = Task.Delay(_dedupeWindow).ContinueWith(_ =>
        {
            lock (_recentPaths) { _recentPaths.Remove(evt.Path); }
        });

        if (_channel.Writer.TryWrite(evt))
        {
            Interlocked.Increment(ref _processed);
            return true;
        }

        Interlocked.Increment(ref _dropped);
        Log.Warning("EventPipeline overflow — dropping event for {Path}", evt.Path);
        return false;
    }

    public void Dispose() => _channel.Writer.TryComplete();
}
