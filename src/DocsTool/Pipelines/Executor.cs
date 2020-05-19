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
            BuildFileSystem = CreateFileSystem(currentPath, site.BuildPath);
            GitFileSystem = new GitFileSystem(
                GitFileSystem.DiscoverRepository(CurrentPath));
        }

        public string CurrentPath { get; }

        public IFileSystem BuildFileSystem { get; }

        public SiteDefinition Site { get; }

        public IFileSystem FileSystem { get; }

        public GitFileSystem GitFileSystem { get; }

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
            var sources = BuildSources();
            var aggregator = new ContentAggregator(
                new MimeDbClassifier(),
                sources);

            var catalog = new Catalog();
            await catalog.Add(aggregator.Enumerate(), cancellationToken);
        }

        private IEnumerable<Directory> BuildSources()
        {
            foreach (var branch in Site.Branches.Keys)
            {
                // current working copy
                if (branch == "__WIP__")
                    yield return FileSystem.GetDirectory(Site.InputPath ?? "");
                else 
                    yield return GitFileSystem.Branch(branch)
                    .GetDirectory(Site.InputPath ?? "");
            }

            //todo: tags
        }
    }
}