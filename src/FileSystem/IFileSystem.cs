using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    public interface IReadOnlyFileSystem
    {
        public ValueTask<IReadOnlyFile?> GetFile(Path path);

        public ValueTask<IReadOnlyDirectory?> GetDirectory(Path path);

        public IAsyncEnumerable<IFileSystemNode> Enumerate(Path path);
    }

    public interface IFileSystem: IReadOnlyFileSystem
    {
        public ValueTask<IFile> GetOrCreateFile(Path path);

        public ValueTask<IDirectory> GetOrCreateDirectory(Path path);

        public ValueTask<IFileSystem> Mount(Path path);

        Task DeleteDir(Path path);
    }

    public interface IFile: IReadOnlyFile
    {
        ValueTask<Stream> OpenWrite();
    }

    public interface IReadOnlyFile : IFileSystemNode
    {
        ValueTask<Stream> OpenRead();
    }

    public interface IDirectory : IReadOnlyDirectory
    {
    }

    public interface IReadOnlyDirectory : IFileSystemNode
    {
        IAsyncEnumerable<IFileSystemNode> Enumerate();
    }

    public interface IFileSystemNode
    {
        Path Path { get; }

        //public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}