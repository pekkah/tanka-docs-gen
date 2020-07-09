using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Globbing;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Tanka.DocsTool.Definitions;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Catalogs
{
    public class ContentAggregator
    {
        private readonly IContentClassifier _classifier;
        private readonly GitFileSystemRoot _git;
        private readonly SiteDefinition _site;
        private readonly IFileSystem _workFileSystem;
        private readonly ILogger<ContentAggregator> _logger;

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
            _logger = Infra.LoggerFactory.CreateLogger<ContentAggregator>();
        }

        public async IAsyncEnumerable<ContentItem> Aggregate(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var _ = _logger.BeginScope(nameof(Aggregate));

            var stack = new Stack<IFileSystemNode>();
            await foreach (var source in BuildSources(cancellationToken))
            {
                await foreach (var node in source.Enumerate(cancellationToken))
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
                                stack.Push(subNode);
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
            using var _ = _logger.BeginScope(nameof(BuildSources));

            var branches = _site.Branches;

            foreach (var (branch, definition) in branches)
            {
                using (_logger.BeginScope($"Source({branch}):"))
                {
                    _logger.LogInformation($"Definition: {JsonSerializer.Serialize(definition)}");

                    var commonInputPaths = definition.InputPath;
                    foreach (var commonInputPath in commonInputPaths)
                    {
                        using var __ = _logger.BeginScope("InputPath({inputPath})", commonInputPath);
                        if (branch == "HEAD")
                        {
                            var status = _git.Repo.RetrieveStatus();

                            // If we have changes then we actually use the current folder
                            // as the source of the truth
                            if (status.IsDirty)
                            {
                                _logger.LogInformation(
                                    "Special branch HEAD is dirty and will fallback to using working copy");
                                var inputDirectory = await _workFileSystem.GetDirectory(commonInputPath);
                                if (inputDirectory != null)
                                    yield return new FileSystemContentSource(
                                        _workFileSystem,
                                        "HEAD",
                                        inputDirectory.Path); // we're mounting the input path
                            }
                            else
                            {
                                _logger.LogInformation("Using HEAD");
                                var head = _git.Head();
                                if (await head.GetDirectory(commonInputPath) != null)
                                    yield return new GitBranchContentSource(head, commonInputPath);
                            }
                        }
                        else
                        {
                            // use globbing to find matching branches
                            _logger.LogInformation("Finding matching branches: {glob}", branch);
                            var glob = Glob.Parse(branch);
                            foreach (var repoBranch in _git.Repo.Branches)
                                if (glob.IsMatch(repoBranch.FriendlyName) || glob.IsMatch(repoBranch.CanonicalName))
                                {
                                    var matchingBranch = _git.Branch(repoBranch.CanonicalName);
                                    _logger.LogInformation("Using branch: {branch}", matchingBranch.FriendlyName);
                                    if (await matchingBranch.GetDirectory(commonInputPath) != null)
                                        yield return new GitBranchContentSource(
                                            matchingBranch,
                                            commonInputPath);
                                }
                        }
                    }
                }
            }

            var tags = _site.Tags;

            foreach (var (tag, definition) in tags)
            {
                using (_logger.BeginScope($"Source({tag}):"))
                {
                    _logger.LogInformation($"Definition: {JsonSerializer.Serialize(definition)}");

                    var inputPaths = definition.InputPath;

                    foreach (var inputPath in inputPaths)
                    {
                        // use globbing to find matching branches
                        _logger.LogInformation("Finding matching tags: {glob}", tag);
                        var glob = Glob.Parse(tag);
                        foreach (var repoBranch in _git.Repo.Tags)
                            if (glob.IsMatch(repoBranch.FriendlyName) || glob.IsMatch(repoBranch.CanonicalName))
                            {
                                var matchingBranch = _git.Tag(repoBranch);
                                _logger.LogInformation("Using branch: {branch}", matchingBranch.FriendlyName);
                                if (await matchingBranch.GetDirectory(inputPath) != null)
                                    yield return new GitBranchContentSource(
                                        matchingBranch,
                                        inputPath);
                            }
                    }
                }
            }
        }
    }
}