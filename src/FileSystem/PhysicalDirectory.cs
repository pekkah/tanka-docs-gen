using System.Collections.Generic;
using System.IO;

namespace Tanka.FileSystem
{
    internal class PhysicalDirectory : IDirectory
    {
        private readonly PhysicalFileSystem _fileSystem;
        private readonly string _fullPath;

        public PhysicalDirectory(PhysicalFileSystem fileSystem, FileSystemPath path, string fullPath)
        {
            Path = path;
            _fileSystem = fileSystem;
            _fullPath = fullPath;
            Metadata = new Metadata()
            {
                ModifiedOn = Directory.GetLastWriteTimeUtc(fullPath),
                Author = string.Empty
            };
        }

        public FileSystemPath Path { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }


        public IAsyncEnumerable<IFileSystemNode> Enumerate()
        {
            return _fileSystem.Enumerate(Path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}