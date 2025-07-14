using System.Collections.Concurrent;
using DotNet.Globbing;
using Tanka.DocsTool.Catalogs;

namespace Tanka.DocsTool.Pipelines;

public class SectionCollector
{
    private readonly ConcurrentBag<Section> _sections = new ConcurrentBag<Section>();
    private readonly IAnsiConsole _console;
    private readonly bool _debug;

    public SectionCollector(IAnsiConsole console, bool debug = false)
    {
        _console = console;
        _debug = debug;
    }

    public async Task Collect(Catalog catalog, ProgressContext progress, BuildContext context)
    {
        var versions = catalog.GetVersions()
            .OrderBy(v => v)
            .ToList();

        var tasks = versions.ToDictionary(v => v, v => progress.AddTask($"Version: {v}", maxValue: 0));

        foreach (var version in versions)
        {
            var task = tasks[version];
            var sectionDefinitionItems = catalog.
                GetContentItems(version, ContentItem.SectionDefinitionType)
                .ToList();

            task.MaxValue = sectionDefinitionItems.Count;

            foreach (var contentItem in sectionDefinitionItems)
            {
                await CollectSection(catalog, contentItem, context);
                task.Increment(1);
            }

            task.StopTask();
        }
    }

    private async Task CollectSection(Catalog catalog, ContentItem contentItem, BuildContext context)
    {
        _console.LogInformation($"Collect({contentItem})");

        var definitionResult = await contentItem.TryParseYaml<SectionDefinition>();

        if (definitionResult.IsFailure)
        {
            // contentItem is passed to Error to associate the error with the file
            // that caused it.
            context.Add(new Error(definitionResult.Error, contentItem));
            return;
        }

        var definition = definitionResult.Value;

        // default to doc type
        if (string.IsNullOrEmpty(definition.Type))
            definition.Type = "doc";

        if (_debug)
            _console.WriteLine($"Section: {definition.ToJson()}");

        var sectionItems = CollectSectionItems(
            definition,
            catalog,
            contentItem);

        var sectionDirectoryPath = contentItem
            .SourceRelativePath
            .GetDirectoryPath();

        var sectionItemsByRelativePath = sectionItems
            .ToDictionary(
                s => s.SourceRelativePath.GetRelative(sectionDirectoryPath),
                s => s
                );

        _sections.Add(new Section(contentItem, definition, sectionItemsByRelativePath));
    }

    private IEnumerable<ContentItem> CollectSectionItems(
        SectionDefinition definition,
        Catalog catalog,
        ContentItem contentItem)
    {
        var sectionDirectoryPath = contentItem.File.Path.GetDirectoryPath();

        var contentItems = catalog.GetContentItems(
            contentItem.Version,
            new[] { "**" },
            $"{sectionDirectoryPath}/**/*");

        var includes = definition.Includes ?? new[] { "**/*" };
        var includeGlobs = includes.Select(Glob.Parse)
            .ToList();

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

                var possibleLocations = new List<FileSystem.FileSystemPath>();
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
                    ContentItem.SectionDefinitionType,
                    possibleLocations).Any())
                {
                    return false;
                }

                return true;
            })
            .Where(item =>
            {
                var itemPath = item.File.Path;
                var relativeItemPath = itemPath.GetRelative(sectionDirectoryPath);

                // apply includes
                var include = includeGlobs.Any(glob => glob.IsMatch(relativeItemPath));

                if (include && _debug)
                    _console.LogDebug($"ContentItem({itemPath} => {contentItem}): include: {include}");

                return include;
            })
            .ToList();
    }

    public IReadOnlyCollection<Section> Sections => _sections;
}
