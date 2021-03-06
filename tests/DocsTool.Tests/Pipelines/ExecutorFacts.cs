﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
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

        [Fact]
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

            /* When */
            var executor = new Executor(
                site, 
                GitRootPath);
            
            await executor.Execute();
            /* Then */
        }

        private static string GetRepoRootWithoutDotGit()
        {
            return Repository.Discover(Environment.CurrentDirectory)
                .Replace(".git", string.Empty);
        }
    }
}