using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Globbing;
using LibGit2Sharp;
using Tanka.DocsTool.Definitions;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Catalogs
{
    public class ContentAggregator
    {
        private readonly SiteDefinition _site;
        private readonly GitFileSystemRoot _git;
        private readonly IFileSystem _workFileSystem;
        private readonly IContentClassifier _classifier;

        public ContentAggregator(
            SiteDefinition site,
            GitFileSystemRoot git,
            IFileSystem workFileSystem,
            IContentClassifier classifier)
        {
            _site = site;
            _git = git;
            _workFileSystem = workFileSystem;
            _classifier = classifier;
        }

        public async IAsyncEnumerable<ContentItem> Aggregate(
            [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            var stack = new Stack<IFileSystemNode>();

            await foreach (var source in BuildSources(cancellationToken))
            {
                await foreach(var node in source.Enumerate(cancellationToken))
                    stack.Push(node);

                while (stack.Count > 0)
                {
                    var node = stack.Pop();

                    switch (node)
                    {
                        case IReadOnlyFile file:
                            yield return await CreateContentItem(source, file);
                            break;
                        case IReadOnlyDirectory dir:
                            await foreach (var subNode in dir.Enumerate()
                                .WithCancellation(cancellationToken))
                            {
                                stack.Push(subNode);
                            }
                            break;
                    }
                }
            }
        }

        private async Task<ContentItem> CreateContentItem(IContentSource source, IReadOnlyFile file)
        {
            await Task.Yield(); //todo: do we do caching here or at build sources
            var type = _classifier.Classify(file);

            return new ContentItem(source, type, file);
        }

        private async IAsyncEnumerable<IContentSource> BuildSources(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var branches = _site.Branches;
            var tags = _site.Tags;

            var commonInputPath = _site.InputPath ?? "docs";

            foreach (var branch in branches)
            {
                if (branch.Key == "HEAD")
                {
                    var status = _git.Repo.RetrieveStatus();

                    // If we have changes then we actually use the current folder
                    // as the source of the truth
                    if (status.IsDirty)
                    {
                        yield return new FileSystemContentSource(
                            await _workFileSystem.Mount(commonInputPath),
                            "HEAD",
                            ""); // we're mounting the input path
                    }
                    else
                    {
                        yield return new GitBranchContentSource(_git.Head(), commonInputPath);
                    }

                    continue;
                }

                // use globbing to find matching branches
                var glob = Glob.Parse(branch.Key);
                foreach (var repoBranch in _git.Repo.Branches)
                {
                    if (glob.IsMatch(repoBranch.FriendlyName))
                    {
                        yield return new GitBranchContentSource(
                            _git.Branch(repoBranch.CanonicalName),
                            commonInputPath);
                    }
                }
            }
        }
    }
}