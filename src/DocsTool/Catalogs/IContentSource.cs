namespace Tanka.DocsTool.Catalogs
{
    public interface IContentSource
    {
        public string Version { get; }

        public FileSystemPath Path { get; }

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken);
    }
}