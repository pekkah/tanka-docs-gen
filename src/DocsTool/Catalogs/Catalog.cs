using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Globbing;
using Microsoft.Extensions.Logging;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class Catalog
    {
        public Catalog()
        {
            _logger = Infra.LoggerFactory.CreateLogger<Catalog>();
        }

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, BlockingCollection<ContentItem>>>
            _contentItems =
                new ConcurrentDictionary<string, ConcurrentDictionary<string, BlockingCollection<ContentItem>>>();

        private ILogger<Catalog> _logger;

        public async Task Add(
            IAsyncEnumerable<ContentItem> aggregate,
            CancellationToken cancellationToken = default)
        {
            _logger.BeginScope(nameof(Add));
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
                (string Version, IReadOnlyDictionary<string, IReadOnlyCollection<ContentItem>> ContentItems)>
            EnumerateVersions()
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

        public IEnumerable<ContentItem> GetContentItems(string version, string type, IEnumerable<string> patterns)
        {
            var globs = patterns.Select(Glob.Parse)
                .ToList();

            if (_contentItems.TryGetValue(version, out var versionCollection))
                if (versionCollection.TryGetValue(type, out var typeCollection))
                    return typeCollection
                        .Where(t => globs.Any(glob => glob.IsMatch(t.File.Path)));

            return Enumerable.Empty<ContentItem>();
        }

        public IEnumerable<ContentItem> GetContentItems(string version, string type, IEnumerable<Path> patterns)
        {
            return GetContentItems(version, type, patterns.Select(p => p.ToString()));
        }

        public IEnumerable<ContentItem> GetContentItems(string version, params string[] typePatterns)
        {
            var versionItems = GetContentItemsOfVersion(version);

            if (versionItems == null)
                yield break;

            var globs = typePatterns.Select(Glob.Parse)
                .ToList();

            foreach (var (type, collection) in versionItems)
            {
                if (!globs.Any(glob => glob.IsMatch(type))) continue;

                foreach (var contentItem in collection)
                {
                    yield return contentItem;
                }
            }
        }

        public IEnumerable<ContentItem> GetContentItems(string version, string[] typePatterns, params Path[] patterns)

        {
            var versionItems = GetContentItemsOfVersion(version);

            if (versionItems == null)
                yield break;

            var typeGlobs = typePatterns.Select(Glob.Parse)
                .ToList();

            var pathGlobs = patterns.Select(p => Glob.Parse(p))
                .ToList();

            foreach (var (type, collection) in versionItems)
            {
                if (!typeGlobs.Any(glob => glob.IsMatch(type))) continue;

                foreach (var contentItem in collection)
                {
                    if (pathGlobs.Any(glob => glob.IsMatch(contentItem.File.Path)))
                        yield return contentItem;
                }
            }
        }

        public IEnumerable<string> GetVersions()
        {
            return _contentItems.Keys;
        }
    }
}