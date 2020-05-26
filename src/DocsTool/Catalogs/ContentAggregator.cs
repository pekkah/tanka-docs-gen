using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.DocsTool.Definitions;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Catalogs
{
    public class ContentAggregator
    {
        private readonly IFileSystem _cache;
        private readonly IContentClassifier _contentClassifier;
        private readonly IReadOnlyFileSystem _currentFolder;
        private readonly GitFileSystemRoot _git;
        private readonly SiteDefinition _site;

        public ContentAggregator(
            IContentClassifier contentClassifier,
            IReadOnlyFileSystem currentFolder,
            IFileSystem cache,
            GitFileSystemRoot git,
            SiteDefinition site)
        {
            _contentClassifier = contentClassifier;
            _currentFolder = currentFolder;
            _cache = cache;
            _git = git;
            _site = site;
        }

        public async IAsyncEnumerable<ContentItem> Enumerate()
        {
            var queue = new Queue<(string SourceName, IReadOnlyDirectory Root)>();

            await foreach (var source in BuildSources())
                queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var source = queue.Dequeue();
                var (name, root) = source;

                await foreach (var node in root.Enumerate())
                    switch (node)
                    {
                        case IReadOnlyDirectory subDirectory:
                            queue.Enqueue((name, subDirectory));
                            break;
                        case IReadOnlyFile file:
                            yield return await CreateContentItem(name, root, file);
                            break;
                    }
            }
        }

        private async IAsyncEnumerable<(string SourceName, IReadOnlyDirectory Root)> BuildSources()
        {
            if (_site.Branches != null)
            {
                foreach (var branchDefinition in _site.Branches)
                {
                    // current working copy
                    if (branchDefinition.Key == "__WIP__")
                    {
                        var dir = await _currentFolder
                            .GetDirectory(_site.InputPath ?? "");

                        if (dir == null)
                            continue;

                        yield return (branchDefinition.Key, dir);
                    }
                    else
                    {
                        var branch = _git
                            .Branch(branchDefinition.Key);

                        var dir = await branch
                            .GetDirectory(_site.InputPath ?? "");

                        if (dir == null)
                            continue;

                        yield return (branchDefinition.Key, dir);
                    }
                }
            }

            //todo: tags
        }

        private async Task<ContentItem> CreateContentItem(
            string sourceName,
            IReadOnlyDirectory directory,
            IReadOnlyFile file)
        {
            var targetDirectoryPath = sourceName / directory.Path;
            var filePath = file.Path;

            var cacheDirectory = await _cache
                .GetOrCreateDirectory(targetDirectoryPath);

            await using var sourceStream = await file.OpenRead();

            // copy to cache
            var cachedFile = await _cache.GetOrCreateFile(sourceName / filePath);
            await using var targetStream = await cachedFile.OpenWrite();
            await sourceStream.CopyToAsync(targetStream);

            // classify
            var type = _contentClassifier.Classify(file);

            return new ContentItem(cacheDirectory, cachedFile, type, sourceName);
        }
    }
}