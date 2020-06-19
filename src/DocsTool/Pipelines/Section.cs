using System.Collections.Generic;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Pipelines
{
    public class Section
    {
        private readonly ContentItem _contentItem;

        public Section(ContentItem contentItem, SectionDefinition definition, IReadOnlyDictionary<Path, ContentItem> contentItems)
        {
            _contentItem = contentItem;
            Definition = definition;
            ContentItems = contentItems;
        }

        public string Id => Definition.Id;

        public string Version => _contentItem.Version;

        public SectionDefinition Definition { get; }

        public Path Path => _contentItem.File.Path.GetDirectoryPath();

        public IReadOnlyDictionary<Path, ContentItem> ContentItems { get; }

        public override string ToString()
        {
            return $"{Id}@{Version} '{Path}'";
        }

        public ContentItem? GetContentItem(Path path)
        {
            if (ContentItems.TryGetValue(path, out var contentItem))
                return contentItem;

            return null;
        }

        public Section WithContentItems(params ContentItem[] contentItems)
        {
            var contentItemsByRelativePath = new Dictionary<Path, ContentItem>();
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