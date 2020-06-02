using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Globbing;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class Catalog
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, BlockingCollection<ContentItem>>>
            _contentItems =
                new ConcurrentDictionary<string, ConcurrentDictionary<string, BlockingCollection<ContentItem>>>();

        public async Task Add(
            IAsyncEnumerable<ContentItem> aggregate,
            CancellationToken cancellationToken = default)
        {
            await foreach (var contentItem in aggregate.WithCancellation(cancellationToken))
            {
                var version = contentItem.Version;
                var type = contentItem.Type;

                var versionCollection = _contentItems
                    .GetOrAdd(version,
                        v => new ConcurrentDictionary<string, BlockingCollection<ContentItem>>());

                var typeCollection = versionCollection
                    .GetOrAdd(type,
                        t => new BlockingCollection<ContentItem>());

                typeCollection.Add(contentItem, cancellationToken);
            }
        }

        public IEnumerable<ContentItem> Enumerate(string version, string type)
        {
            if (_contentItems.TryGetValue(version, out var versionCollection))
                if (versionCollection.TryGetValue(type, out var typeCollection))
                    return typeCollection;

            return Enumerable.Empty<ContentItem>();
        }

        public IReadOnlyDictionary<string, IReadOnlyCollection<ContentItem>>? GetContentItemsOfVersion(string version)
        {
            if (_contentItems.TryGetValue(version, out var versionCollection))
            {
                var result = new Dictionary<string, IReadOnlyCollection<ContentItem>>();
                foreach (var typeCollection in versionCollection) 
                    result[typeCollection.Key] = typeCollection.Value;

                return result;
            }

            return null;
        }

        public IEnumerable<
                (string Version, IReadOnlyDictionary<string, IReadOnlyCollection<ContentItem>> ContentItems)> EnumerateVersions()
        {
            foreach (var (version, collection) in _contentItems)
            {
                var result = new Dictionary<string, IReadOnlyCollection<ContentItem>>();
                foreach (var typeCollection in collection) 
                    result[typeCollection.Key] = typeCollection.Value;

                yield return (version, result);
            }
        }

        public ContentItem? GetContentItem(string version, string type, Path path)
        {
            if (_contentItems.TryGetValue(version, out var versionCollection))
                if (versionCollection.TryGetValue(type, out var typeCollection))
                    return typeCollection.SingleOrDefault(t => t.File.Path == path);

            return null;
        }

        public IEnumerable<ContentItem> GetContentItems(string version, string type, string pattern)
        {
            var glob = Glob.Parse(pattern);

            if (_contentItems.TryGetValue(version, out var versionCollection))
                if (versionCollection.TryGetValue(type, out var typeCollection))
                    return typeCollection.Where(t => glob.IsMatch(t.File.Path));

            return Enumerable.Empty<ContentItem>();
        }

        public IEnumerable<string> GetVersions()
        {
            return _contentItems.Keys;
        }
    }
}