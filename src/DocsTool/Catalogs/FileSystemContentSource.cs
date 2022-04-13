namespace Tanka.DocsTool.Catalogs
{
    public class FileSystemContentSource : IContentSource
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _path;

        public FileSystemContentSource(IFileSystem fileSystem, string version, FileSystemPath path)
        {
            _fileSystem = fileSystem;
            _path = path;
            Version = version;
        }

        public string Version { get; }
        public FileSystemPath Path => _path;

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken) => _fileSystem.Enumerate(_path);
    }
}