using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Pipelines
{
    public class SectionCollector
    {
        private readonly ConcurrentBag<Section> _sections = new ConcurrentBag<Section>();

        public async Task Collect(Catalog catalog)
        {
            foreach (var (_, contentItemsByType) in catalog.EnumerateVersions())
            {
                foreach (var (_, items) in contentItemsByType)
                {
                    foreach (var contentItem in items)
                    {
                        if (contentItem.File.Path.GetFileName() == "tanka-docs-section.yml")
                        {
                            await CollectSection(catalog, contentItem);
                        }
                    }
                }
            }
        }

        private async Task CollectSection(Catalog catalog, ContentItem contentItem)
        {
            var definition = await contentItem.ParseYaml<SectionDefinition>();
            var sectionItems = CollectSectionItems(
                catalog, 
                contentItem);
            
            var sectionDirectoryPath = contentItem.File.Path.GetDirectoryPath();

            var sectionItemsByRelativePath = sectionItems
                .ToDictionary(s => 
                        s.File.Path.GetRelative(sectionDirectoryPath),
                    s => s
                    );

            _sections.Add(new Section(contentItem, definition, sectionItemsByRelativePath));
        }

        private IEnumerable<ContentItem> CollectSectionItems(
            Catalog catalog, 
            ContentItem contentItem)
        {
            var sectionDirectoryPath = contentItem.File.Path.GetDirectoryPath();

            var contentItems = catalog.GetContentItems(
                contentItem.Version,
                new []{"**"},
                $"{sectionDirectoryPath}/**/*");

            // filter out items belonging to sections under this section
            return contentItems
                .Where(item =>
                {
                    // exclude folders with section definitions
                    var itemPath = item.File.Path.GetDirectoryPath();
                    var relativeItemPath = itemPath.GetRelative(sectionDirectoryPath);
                    var relativePathSegments = relativeItemPath
                        .EnumerateSegments()
                        .ToList();

                    var possibleLocations = new List<FileSystem.Path>();
                    var temp = string.Empty;
                    foreach (var relativePathSegment in relativePathSegments)
                    {
                        temp += relativePathSegment;
                        possibleLocations.Add(sectionDirectoryPath / temp / "tanka-docs-section.yml");
                    }

                    // include items in same folder as the section definition
                    if (itemPath == sectionDirectoryPath)
                        return true;

                    if (catalog.GetContentItems(
                        contentItem.Version,
                        "text/yaml",
                        possibleLocations).Any())
                    {
                        return false;
                    }

                    return true;
                }).ToList();
        }

        public IReadOnlyCollection<Section> Sections => _sections;
    }
}