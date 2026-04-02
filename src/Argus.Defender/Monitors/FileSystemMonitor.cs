// src\Argus.Defender\Monitors\FileSystemMonitor.cs
using Serilog;

namespace Argus.Defender.Monitors;

public sealed class FileSystemMonitor : IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly EventPipeline _pipeline;
    public event EventHandler<string>? FileChanged;

    public FileSystemMonitor(IEnumerable<string> watchPaths, EventPipeline? pipeline = null)
    {
        _pipeline = pipeline ?? new EventPipeline();
        foreach (var path in watchPaths)
        {
            if (!Directory.Exists(path))
            {
                Log.Warning("Watch path does not exist, skipping: {Path}", path);
                continue;
            }

            var watcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                InternalBufferSize = 65536, // 64 KB — required to avoid silent event drops
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = false // Enabled in StartAsync
            };

            watcher.Created += OnFileEvent;
            watcher.Changed += OnFileEvent;
            watcher.Deleted += OnFileEvent;
            watcher.Renamed += OnRenameEvent;
            _watchers.Add(watcher);
        }
    }

    public Task StartAsync(CancellationToken ct)
    {
        foreach (var w in _watchers)
            w.EnableRaisingEvents = true;

        return Task.Delay(Timeout.Infinite, ct)
            .ContinueWith(_ => { foreach (var w in _watchers) w.EnableRaisingEvents = false; },
                TaskScheduler.Default);
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        var type = e.ChangeType switch
        {
            WatcherChangeTypes.Created => MonitorEventType.FileCreated,
            WatcherChangeTypes.Changed => MonitorEventType.FileModified,
            WatcherChangeTypes.Deleted => MonitorEventType.FileDeleted,
            _ => MonitorEventType.FileModified
        };

        _pipeline.TryPublish(new MonitorEvent { Type = type, Path = e.FullPath });
        FileChanged?.Invoke(this, e.FullPath);
    }

    private void OnRenameEvent(object sender, RenamedEventArgs e)
    {
        _pipeline.TryPublish(new MonitorEvent
        {
            Type = MonitorEventType.FileRenamed,
            Path = e.FullPath,
            OldPath = e.OldFullPath
        });
        FileChanged?.Invoke(this, e.FullPath);
    }

    public void Dispose()
    {
        foreach (var w in _watchers) w.Dispose();
    }
}
