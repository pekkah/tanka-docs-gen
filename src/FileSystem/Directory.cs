using System.Collections.Generic;

namespace Tanka.FileSystem
{
    public class Directory : IFileSystemNode
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public Directory(IReadOnlyFileSystem fileSystem, Path path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public Path Path { get; }

        public IAsyncEnumerable<IFileSystemNode> Enumerate()
        {
            return _fileSystem.EnumerateDirectory(this);
        }

        public override string ToString()
        {
            return Path.ToString();
        }
    }
}