using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Pipelines;
using Xunit;

namespace Tanka.DocsTool.Tests.Pipelines
{
    public class ExecutorFacts
    {
        public ExecutorFacts()
        {
            GitRootPath = GetRepoRootWithoutDotGit();
        }

        public string GitRootPath { get; set; }

        [Fact(Skip ="Refactor so this runs on memory")]
        public async Task Execute()
        {
            /* Given */
            var site = new SiteDefinition
            {
                Title = "ExecutorFacts",
                OutputPath = "output",
                BuildPath = "_build",
                Branches = new Dictionary<string, BranchDefinition>
                {
                    ["HEAD"] = new BranchDefinition()
                    {
                        InputPath = new[] { "ui-bundle", "docs-v2", "src"}
                    }
                },
                Tags = new Dictionary<string, BranchDefinition>()
                {
                    ["0.*"] = new BranchDefinition()
                    {
                        InputPath = new[] { "ui-bundle", "docs-v2", "src" }
                    }
                }
            };

            var services = new ServiceCollection()
                .AddSingleton<IAnsiConsole>(AnsiConsole.Create(new AnsiConsoleSettings()
                {
                    Interactive = InteractionSupport.No
                }))
                .AddDefaultPipeline()
                .BuildServiceProvider();

            /* When */
            var executor = new PipelineExecutor(
                new BuildSiteCommand.Settings()
                );

            await executor.Execute(new PipelineBuilder(services)
                .UseDefault(), 
                site,
                GitRootPath);

            /* Then */
        }

        private static string GetRepoRootWithoutDotGit()
        {
            return Repository.Discover(Environment.CurrentDirectory)
                .Replace(".git", string.Empty);
        }
    }
}