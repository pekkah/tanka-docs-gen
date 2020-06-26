using System.Collections.Generic;
using System.Threading;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class FileSystemContentSource : IContentSource
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _path;

        public FileSystemContentSource(IFileSystem fileSystem, string version, Path path)
        {
            _fileSystem = fileSystem;
            _path = path;
            Version = version;
        }

        public string Version { get; }
        public Path Path => _path;

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken)
        {
            return _fileSystem.Enumerate(_path);
        }
    }
}