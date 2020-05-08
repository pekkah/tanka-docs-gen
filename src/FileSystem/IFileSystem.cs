using System.Collections.Generic;
using System.IO.Pipelines;

namespace Tanka.FileSystem
{
    public interface IReadOnlyFileSystem
    {
        IAsyncEnumerable<IFileSystemNode> EnumerateDirectory(Directory directory);

        IAsyncEnumerable<IFileSystemNode> EnumerateRoot();

        PipeReader OpenRead(File file);

        Directory GetDirectory(Path path);

        File GetFile(Path path);
    }

    public interface IFileSystem : IReadOnlyFileSystem
    {
        PipeWriter OpenWrite(File file);
    }
}