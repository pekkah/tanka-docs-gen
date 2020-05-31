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

        public IReadOnlyFile File { get; }

        public string Name => File.Path.GetFileName();
    }
}