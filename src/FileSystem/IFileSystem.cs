using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    public interface IReadOnlyFileSystem
    {
        public ValueTask<IReadOnlyFile?> GetFile(FileSystemPath path);

        public ValueTask<IReadOnlyDirectory?> GetDirectory(FileSystemPath path);

        public IAsyncEnumerable<IFileSystemNode> Enumerate(FileSystemPath path);
    }

    public interface IFileSystem: IReadOnlyFileSystem
    {
        public ValueTask<IFile> GetOrCreateFile(FileSystemPath path);

        public ValueTask<IDirectory> GetOrCreateDirectory(FileSystemPath path);

        public ValueTask<IFileSystem> Mount(FileSystemPath path);

        Task DeleteDir(FileSystemPath path);

        Task CleanDirectory(FileSystemPath path);
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
        FileSystemPath Path { get; }

        //public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}