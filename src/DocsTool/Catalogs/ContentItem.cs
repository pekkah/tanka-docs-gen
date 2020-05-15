using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class ContentItem
    {
        public ContentItem(Directory directory, File file, string type)
        {
            Directory = directory;
            File = file;
            Type = type;
        }

        public string Type { get; }

        public File File { get; }

        public Directory Directory { get; }
    }
}