using System.Collections.Generic;
using System.Linq;
using DotNet.Globbing;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Pipelines
{
    public class Section
    {
        private readonly ContentItem _contentItem;

        public Section(
            ContentItem contentItem, 
            SectionDefinition definition,
            IReadOnlyDictionary<Path, ContentItem> contentItems)
        {
            _contentItem = contentItem;
            Definition = definition;
            ContentItems = contentItems;
        }

        public string Id => Definition.Id;

        public string Type => Definition.Type;

        public string Version => _contentItem.Version;

        public SectionDefinition Definition { get; }

        public Path Path => _contentItem.SourceRelativePath.GetDirectoryPath();

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

        public IEnumerable<(Path RelativePath, ContentItem ContentItem)> GetContentItems(params Path[] patterns)
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