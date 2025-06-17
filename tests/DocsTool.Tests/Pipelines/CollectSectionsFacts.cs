using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Spectre.Console;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;
using Xunit;

namespace Tanka.DocsTool.Tests.Pipelines
{
    public class CollectSectionsFacts: IDisposable
    {
        public CollectSectionsFacts()
        {
            var root = GetRepoRootWithoutDotGit();
            _repo = GitFileSystemRoot.DiscoverRepo(root);
            var git = new GitFileSystemRoot(_repo);
            _catalog = new Catalog();
            var site = new SiteDefinition
            {
                Title = "Run pipeline",
                OutputPath = "output",
                BuildPath = "_build",
                Branches = new Dictionary<string, BranchDefinition>
                {
                    ["HEAD"] = new BranchDefinition()
                    {
                        InputPath = new[] { "docs-v2", "ui-bundle" }
                    }
                }
            };

            _console = AnsiConsole.Create(new AnsiConsoleSettings()
            {
                Interactive = InteractionSupport.No,
            });

            _aggregator = new ContentAggregator(
                site, 
                git,
                new PhysicalFileSystem(root),
                new MimeDbClassifier(),
                _console);
        }

        private readonly Catalog _catalog;
        private readonly Repository _repo;
        private readonly IAnsiConsole _console;
        private readonly ContentAggregator _aggregator;


        [Fact]
        public Task From_root_of_the_path()
        {
            return  _console.Progress()
                .StartAsync(async progress => {
                    /* Given */
                    var collector = new SectionCollector(_console, true);
                    var root = GetRepoRootWithoutDotGit();
                    var buildContext = new BuildContext(new SiteDefinition(), root);
                    await _catalog.Add(_aggregator.Aggregate(buildContext, progress, CancellationToken.None));

                    /* When */
                    await collector.Collect(_catalog, progress, buildContext);

                    /* Then */
                    Assert.Single(collector.Sections, section => section.Id == "root");
                });

        }

        [Fact]
        public Task From_subpath_of_the_root_path()
        {
            return _console.Progress()
                .StartAsync(async progress =>
                {
                    /* Given */
                    var collector = new SectionCollector(_console);
                    var root = GetRepoRootWithoutDotGit();
                    var buildContext = new BuildContext(new SiteDefinition(), root);
                    await _catalog.Add(_aggregator.Aggregate(buildContext, progress, CancellationToken.None));

                    /* When */
                    await collector.Collect(_catalog, progress, buildContext);

                    /* Then */
                    Assert.Single(collector.Sections, section => section.Id == "structure");
                });
        }

        public void Dispose()
        {
            _repo.Dispose();
        }

        private static string GetRepoRootWithoutDotGit()
        {
            return Repository.Discover(Environment.CurrentDirectory)
                .Replace(".git", string.Empty);
        }
    }
}