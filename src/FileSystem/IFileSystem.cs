using System.Collections.Generic;
using System.IO.Pipelines;

namespace FileSystem
{
    public interface IFileSystem
    {
        IAsyncEnumerable<IFileSystemNode> EnumerateDirectory(Directory directory);

        IAsyncEnumerable<IFileSystemNode> EnumerateRoot();

        PipeReader OpenRead(File file);

        PipeWriter OpenWrite(File file);
    }
}