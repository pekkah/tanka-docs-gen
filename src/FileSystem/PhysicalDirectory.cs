using System.Collections.Generic;

namespace Tanka.FileSystem
{
    internal class PhysicalDirectory : IDirectory
    {
        private readonly PhysicalFileSystem _fileSystem;

        public PhysicalDirectory(PhysicalFileSystem fileSystem, Path path)
        {
            Path = path;
            _fileSystem = fileSystem;
        }

        public Path Path { get; }


        public IAsyncEnumerable<IFileSystemNode> Enumerate()
        {
            return _fileSystem.Enumerate(Path);
        }
    }
}