using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
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
            /* Build catalog */
            var aggregator = new ContentAggregator(
                new MimeDbClassifier(),
                FileSystem,
                await CacheFileSystem.Mount("content"),
                GitRoot,
                Site);

            var catalog = new Catalog();
            await catalog.Add(aggregator.Enumerate(), cancellationToken);
        }

    }
}