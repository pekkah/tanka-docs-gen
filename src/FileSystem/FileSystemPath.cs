using System;
using System.Collections.Generic;

namespace Tanka.FileSystem;

public readonly struct FileSystemPath
{
    private readonly string _path;

    public FileSystemPath(string path) => _path = PathHelpers.Normalize(path);

    public FileSystemPath Combine(in FileSystemPath path) => new FileSystemPath(PathHelpers.Combine(_path, path._path));

    public static FileSystemPath operator /(in FileSystemPath left, in FileSystemPath right) => left.Combine(right);

    public static FileSystemPath operator -(in FileSystemPath left, in FileSystemPath right)
    {
        var leftSegments = left.Segments();
        var rightSegments = right.Segments();

        var i = 0;
        for (; i < leftSegments.Length; i++)
        {
            if (i >= rightSegments.Length)
            {
                break;
            }

            var leftSegment = leftSegments[i];
            var rightSegment = rightSegments[i];

            if (leftSegment != rightSegment)
                break;
        }

        var resultSegments = leftSegments.Slice(i);
        var resultPath = string.Join('/', resultSegments.ToArray());
        return resultPath;
    }

    public ReadOnlySpan<string> Segments() => _path.Split('/');

    public IEnumerable<string> EnumerateSegments() => _path.Split('/');

    public static implicit operator string(in FileSystemPath path) => path._path;

    public static implicit operator FileSystemPath(string path) => new FileSystemPath(path);

    public FileSystemPath GetRelative(in FileSystemPath relativeTo)
    {
        if (relativeTo == "")
            return this;

        return System.IO.Path.GetRelativePath(relativeTo, _path);
    }

    public override string ToString() => _path;

    public bool IsSubPathOf(in FileSystemPath path) => StartsWith(path);

    public bool StartsWith(in FileSystemPath path) => _path.StartsWith(path._path);

    public FileSystemPath GetDirectoryPath()
    {
        var path = System.IO.Path.GetDirectoryName(_path);

        if (string.IsNullOrEmpty(path))
            return "";

        return path;
    }

    public FileSystemPath GetFileName() => System.IO.Path.GetFileName(_path);

    public FileSystemPath GetExtension() => System.IO.Path.GetExtension(_path);

    public FileSystemPath ChangeExtension(string? extension) => System.IO.Path.ChangeExtension(_path, extension);
}
