using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class ContentItem
    {
        public ContentItem(IReadOnlyDirectory directory, IReadOnlyFile file, string type, string version)
        {
            Directory = directory;
            File = file;
            Type = type;
            Version = version;
        }

        public string Type { get; }

        public string Version { get; }

        public IReadOnlyFile File { get; }

        public IReadOnlyDirectory Directory { get; }
    }
}