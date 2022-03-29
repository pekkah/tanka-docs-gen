using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class CachingContentSource : IContentSource
    {
        private readonly IFileSystem _cache;
        private readonly IContentSource _source;

        public CachingContentSource(IFileSystem cache, IContentSource source)
        {
            _cache = cache;
            _source = source;
        }

        public string Version => _source.Version;

        public Path Path => _source.Path;

        public async IAsyncEnumerable<IFileSystemNode> Enumerate(
            [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            await foreach (var node in _source.Enumerate(cancellationToken))
            {
                switch (node)
                {
                    case IReadOnlyFile sourceFile:
                        yield return await CacheFile(sourceFile);
                        break;
                }
            }
        }

        private async Task<IFileSystemNode> CacheFile(IReadOnlyFile sourceFile)
        {
            if (await _cache.GetFile(sourceFile.Path) != null)
                throw new InvalidOperationException(
                    $"File {sourceFile.Path} is already cached.");

            // create directory
            await _cache.GetOrCreateDirectory(sourceFile.Path.GetDirectoryPath());

            // copy source to target
            var targetFile = await _cache.GetOrCreateFile(sourceFile.Path);
            await using var targetStream = await targetFile.OpenWrite();
            await using var sourceStream = await sourceFile.OpenRead();
            await sourceStream.CopyToAsync(targetStream);



            return targetFile;
        }
    }
}