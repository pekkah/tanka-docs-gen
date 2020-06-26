using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class ContentItem
    {
        private readonly IContentSource _source;

        public ContentItem(IContentSource source, string type, IReadOnlyFile file)
        {
            Type = type;
            File = file;
            _source = source;
        }

        public string Type { get; }

        public string Version => _source.Version;

        public Path SourcePath => _source.Path;

        public Path SourceRelativePath => File.Path.GetRelative(SourcePath);

        public IReadOnlyFile File { get; }

        public string Name => File.Path;

        public override string ToString()
        {
            return $"{Type} {Name}@{Version}";
        }

        public ContentItem WithFile(IFile file, string? type = null)
        {
            return new ContentItem(
                _source,
                type ?? Type,
                file);
        }
    }
}