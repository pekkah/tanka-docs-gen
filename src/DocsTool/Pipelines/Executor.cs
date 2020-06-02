using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Markdig.Syntax.Inlines;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Navigation;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;
using Path = System.IO.Path;

namespace Tanka.DocsTool.Pipelines
{
    public class Executor
    {
        public Executor(
            SiteDefinition site,
            string currentPath)
        {
            CurrentPath = currentPath;
            Site = site;
            FileSystem = new PhysicalFileSystem(CurrentPath);
            CacheFileSystem = CreateFileSystem(currentPath, site.BuildPath);
            GitRoot = GitFileSystemRoot.Discover(CurrentPath);
        }

        public string CurrentPath { get; }

        public IFileSystem CacheFileSystem { get; }

        public SiteDefinition Site { get; }

        public IFileSystem FileSystem { get; }

        public GitFileSystemRoot GitRoot { get; }

        private static IFileSystem CreateFileSystem(string rootPath, string? inputPath)
        {
            rootPath = GetRootedPath(rootPath, inputPath);
            return new PhysicalFileSystem(rootPath);
        }

        private static string GetRootedPath(string rootPath, string? inputPath)
        {
            if (!string.IsNullOrEmpty(inputPath))
            {
                if (Path.IsPathRooted(inputPath))
                    rootPath = inputPath;
                else
                    rootPath = Path.GetFullPath(inputPath, rootPath);
            }

            return rootPath;
        }

        public async Task Execute(CancellationToken cancellationToken = default)
        {
            var contentCache  = await CacheFileSystem.Mount("content");
            var pageHtmlCache = await CacheFileSystem.Mount("content-html"); 

            // quickly exit if no index section
            if (!Site.IndexSection.IsXref)
            {
                throw new InvalidOperationException(
                    $"Could not find index section: '{Site.IndexSection}'. " +
                    "Index section must be an xref.");
            }

            var aggregator = new ContentAggregator(
                Site,
                GitRoot,
                FileSystem,
                new MimeDbClassifier());

            var catalog = new Catalog();
            await catalog.Add(aggregator.Aggregate(cancellationToken), cancellationToken);


            var siteModel = await BuildSite(catalog);

        }

        private async Task<SiteModel> BuildSite(Catalog catalog)
        {
            var sections = new Dictionary<string, Dictionary<string, SectionModel>>();
            await foreach (var section in BuildSections(catalog))
            {
                if (!sections.ContainsKey(section.Version))
                    sections[section.Version] = new Dictionary<string, SectionModel>(1);

                sections[section.Version].Add(section.Definition.Id, section);
            }

            return new SiteModel();
        }

        private async IAsyncEnumerable<SectionModel> BuildSections(Catalog catalog)
        {
            foreach (var version in catalog.GetVersions())
            {
                var sectionDefinitions = await Task.WhenAll<SectionDefinition>(catalog
                    .GetContentItems(version, "text/yaml", "tanka-docs-section.*")
                    .Select(ci => ci.ParseYaml<SectionDefinition>()));

                foreach (var sectionDefinition in sectionDefinitions)
                {
                    var sectionNavigation = await BuildSectionNavigation(
                        sectionDefinition,
                        version,
                        catalog);

                    yield return new SectionModel(version, sectionDefinition, sectionNavigation, new ContentItem[0]);
                }
            }
        }

        private async Task<IReadOnlyCollection<NavigationItem>> BuildSectionNavigation(
            SectionDefinition sectionDefinition,
            string version,
            Catalog catalog)
        {
            //todo: allow referencing other nav docs in difference versions
            var navContentItems = sectionDefinition.Nav
                .Where(link => link.IsXref)
                .Select(link => catalog.GetContentItem(version, "text/markdown", link.Xref!.Value.Path) ?? throw new InvalidOperationException(
                    $"Could not find text/markdown ContentItem for xref '{link.Xref}"));

            /* Load nav files as markdown ast */
            var navDocuments = await Task.WhenAll(navContentItems.Select(ci => ci.ParseMarkdown()));

            /* Validate each document has a unordered list or lists */
            IReadOnlyCollection<NavigationItem> navItems = NavigationBuilder.Build(navDocuments);

            return navItems;
        }
    }

    public class SiteModel
    {
        public SiteDefinition Definition { get; set; }

        public SectionModel Index { get; set; }
    }

    public class SectionModel
    {
        public SectionModel(string version, SectionDefinition definition, IReadOnlyCollection<NavigationItem> navigation, IReadOnlyCollection<ContentItem> pages)
        {
            Version = version;
            Definition = definition;
            Navigation = navigation;
        }

        public string Version { get; }

        public SectionDefinition Definition { get;  }

        public IReadOnlyCollection<NavigationItem> Navigation { get; }

        //private IReadOnlyDictionary<string, ContentItem> PageFiles { get; }
    }
}