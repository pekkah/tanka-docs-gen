using DotNet.Globbing;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Pipelines
{
    public class Section
    {
        private readonly ContentItem _contentItem;

        public Section(
            ContentItem contentItem,
            SectionDefinition definition,
            IReadOnlyDictionary<FileSystemPath, ContentItem> contentItems)
        {
            _contentItem = contentItem;
            Definition = definition;
            ContentItems = contentItems;
        }

        public string Id => Definition.Id;

        public string Type => Definition.Type;

        public string Version => _contentItem.Version;

        public SectionDefinition Definition { get; }

        public Link IndexPage => Definition.IndexPage;

        public string Title => Definition.Title;

        public FileSystemPath Path => _contentItem.SourceRelativePath.GetDirectoryPath();

        public IReadOnlyDictionary<FileSystemPath, ContentItem> ContentItems { get; }

        public override string ToString() => $"{Id}@{Version} '{Path}'";

        public ContentItem? GetContentItem(FileSystemPath path)
        {
            if (ContentItems.TryGetValue(path, out var contentItem))
                return contentItem;

            return null;
        }

        public IEnumerable<(FileSystemPath RelativePath, ContentItem ContentItem)> GetContentItems(params FileSystemPath[] patterns)
        {
            var globs = patterns.Select(p => Glob.Parse(p))
                .ToList();

            foreach (var (path, contentItem) in ContentItems)
            {
                if (globs.Any(g => g.IsMatch(path)))
                    yield return (path, contentItem);
            }
        }

        public Section WithContentItems(params ContentItem[] contentItems)
        {
            var contentItemsByRelativePath = new Dictionary<FileSystemPath, ContentItem>();
            foreach (var contentItem in contentItems)
            {
                var relativePath = contentItem.File.Path.GetRelative(Path);
                contentItemsByRelativePath.Add(relativePath, contentItem);
            }

            return new Section(
                _contentItem,
                Definition,
                contentItemsByRelativePath);
        }
    }
}