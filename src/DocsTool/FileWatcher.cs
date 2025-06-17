using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.DocsTool;

public class FileWatcher : IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private Func<FileChange, Task>? _onChanged;
    private CancellationTokenSource? _debounceCts;
    private CancellationToken _cancellationToken;

    public void Start(IEnumerable<string> paths, Func<FileChange, Task> onChanged, CancellationToken cancellationToken = default)
    {
        _onChanged = onChanged;
        _cancellationToken = cancellationToken;
        var uniquePaths = paths.Distinct();

        foreach (var path in uniquePaths)
        {
            if (File.Exists(path))
            {
                var directory = Path.GetDirectoryName(path);
                var file = Path.GetFileName(path);

                if (string.IsNullOrEmpty(directory)) continue;

                var watcher = new FileSystemWatcher(directory)
                {
                    Filter = file,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                Subscribe(watcher);
                _watchers.Add(watcher);
            }
            else if (Directory.Exists(path))
            {
                var watcher = new FileSystemWatcher(path)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };
                Subscribe(watcher);
                _watchers.Add(watcher);
            }
        }
    }

    private void Subscribe(FileSystemWatcher watcher)
    {
        watcher.Changed += OnFileSystemEvent;
        watcher.Created += OnFileSystemEvent;
        watcher.Deleted += OnFileSystemEvent;
        watcher.Renamed += OnFileSystemEvent;
    }

    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        if (_cancellationToken.IsCancellationRequested)
            return;
            
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();

        // Create a combined cancellation token
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, _debounceCts.Token);

        Task.Delay(100, combinedCts.Token)
            .ContinueWith(async t =>
            {
                if (t.IsCanceled || _onChanged is null || _cancellationToken.IsCancellationRequested)
                    return;

                var changeType = e.ChangeType switch
                {
                    WatcherChangeTypes.Changed => FileChangeType.Changed,
                    WatcherChangeTypes.Created => FileChangeType.Created,
                    WatcherChangeTypes.Deleted => FileChangeType.Deleted,
                    WatcherChangeTypes.Renamed => FileChangeType.Renamed,
                    _ => throw new ArgumentOutOfRangeException()
                };

                try
                {
                    await _onChanged(new FileChange { FullPath = e.FullPath, ChangeType = changeType });
                }
                catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
                {
                    // Expected during shutdown
                }
            }, TaskScheduler.Default);
    }

    public void Stop()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
        
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _watchers.Clear();
    }

    public void Dispose()
    {
        Stop();
    }
}

public enum FileChangeType
{
    Changed,
    Created,
    Deleted,
    Renamed
}

public readonly struct FileChange
{
    public required string FullPath { get; init; }
    public required FileChangeType ChangeType { get; init; }
} 