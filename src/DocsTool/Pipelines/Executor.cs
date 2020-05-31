using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            await foreach (var contentItem in aggregator.Aggregate(cancellationToken))
            {

            }

        }

        private Task<SiteModel> BuildSite(IReadOnlyDictionary<string, SectionDefinition> sectionDefinitions)
        {
            return null;
        }

        private IAsyncEnumerable<SectionModel> BuildSections(IReadOnlyDictionary<string, SectionDefinition> sectionDefinitions)
        {
            return null;
        }
    }

    public class SiteModel
    {
        public SiteDefinition Definition { get; set; }

        public SectionModel Index { get; set; }
    }

    public class SectionModel
    {
        public SectionDefinition Definition { get;  }

        public NavigationItem[] Navigation { get; }

        //private IReadOnlyDictionary<string, ContentItem> PageFiles { get; }
    }
}