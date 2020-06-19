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
using Tanka.DocsTool.UI;
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
            await CacheFileSystem.DeleteDir("content");
            await CacheFileSystem.GetOrCreateDirectory("content");

            await CacheFileSystem.DeleteDir("content-html");
            await CacheFileSystem.GetOrCreateDirectory("content-html");

            var outputPath = GetRootedPath(CurrentPath, Site.OutputPath);
            await FileSystem.DeleteDir(outputPath);
            await FileSystem.GetOrCreateDirectory(outputPath);

            RawCache = await CacheFileSystem.Mount("content");
            PageCache = await CacheFileSystem.Mount("content-html");
            OutputFs = await FileSystem.Mount(outputPath);

            /* Aggregate content based on branches, tags and etc given in site definition */
            var aggregator = new ContentAggregator(
                Site,
                GitRoot,
                FileSystem,
                new MimeDbClassifier());

            var catalog = new Catalog();

            /* Add content */
            await catalog.Add(aggregator.Aggregate(cancellationToken), cancellationToken);

            /* Site */
            var site = await BuildSite(catalog, Site);
            
            /* UI */
            var ui = new UiBuilder(PageCache, OutputFs);
            await ui.BuildSite(site, new HandlebarsUiBundle(await FileSystem.Mount("ui-bundle")));

        }

        private async Task<Site> BuildSite(Catalog catalog, SiteDefinition definition)
        {
            var sectionCollector = new SectionCollector();
            await sectionCollector.Collect(catalog);

            var builder = new SiteBuilder(definition)
                .Add(sectionCollector.Sections);

            return builder.Build();
        }

        public IFileSystem OutputFs { get; set; }

        public IFileSystem PageCache { get; set; }

        public IFileSystem RawCache { get; set; }
    }
}