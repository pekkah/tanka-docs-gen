using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;
using Xunit;

namespace Tanka.DocsTool.Tests.Pipelines
{
    public class SiteBuilderFacts : IDisposable
    {
        public SiteBuilderFacts()
        {
            var root = GetRepoRootWithoutDotGit();
            _repo = GitFileSystemRoot.DiscoverRepo(root);
            var git = new GitFileSystemRoot(_repo);
            _catalog = new Catalog();
            _site = new SiteDefinition
            {
                Title = "Run pipeline",
                InputPath = $"docs-v2",
                OutputPath = "output",
                BuildPath = "_build",
                Branches = new Dictionary<string, string>
                {
                    ["HEAD"] = "??",
                }
            };

            _aggregator = new ContentAggregator(
                _site,
                git,
                new PhysicalFileSystem(root),
                new MimeDbClassifier());
        }

        private readonly Catalog _catalog;
        private readonly Repository _repo;
        private readonly ContentAggregator _aggregator;
        private readonly SiteDefinition _site;


        [Fact]
        public async Task Build_site()
        {
            /* Given */
            var collector = new SectionCollector();
            await _catalog.Add(_aggregator.Aggregate(CancellationToken.None));
            await collector.Collect(_catalog);

            var sut = new SiteBuilder(_site);
            
            /* When */
            var site = sut.Add(collector.Sections)
                .Build();

            /* Then */
            Assert.NotEmpty(site.Versions);

        }

        private static string GetRepoRootWithoutDotGit()
        {
            return Repository.Discover(Environment.CurrentDirectory)
                .Replace(".git", string.Empty);
        }

        public void Dispose()
        {
            _repo.Dispose();
        }
    }
}