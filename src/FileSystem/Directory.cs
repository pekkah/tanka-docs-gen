using System.Collections.Generic;

namespace Tanka.FileSystem
{
    public class Directory : IFileSystemNode
    {
        private readonly IFileSystem _fileSystem;

        public Directory(IFileSystem fileSystem, Path path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public Path Path { get; }

        public IAsyncEnumerable<IFileSystemNode> Enumerate()
        {
            return _fileSystem.EnumerateDirectory(this);
        }
    }
}