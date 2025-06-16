using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tanka.FileSystem;

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

public interface IFileWatcher : IDisposable
{
    void Start(IEnumerable<string> paths, Func<FileChange, Task> onChanged);

    void Stop();
} 