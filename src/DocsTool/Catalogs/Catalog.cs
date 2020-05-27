using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.DocsTool.Catalogs
{
    public class Catalog
    {
        private readonly ConcurrentDictionary<string, BlockingCollection<ContentItem>> _items =
            new ConcurrentDictionary<string, BlockingCollection<ContentItem>>();

        public async Task Add(
            IAsyncEnumerable<ContentItem> aggregate,
            CancellationToken cancellationToken)
        {
            await foreach (var contentItem in aggregate.WithCancellation(cancellationToken))
            {
                var collection = _items
                    .GetOrAdd(
                        contentItem.Type,
                        _ => new BlockingCollection<ContentItem>());

                collection.Add(contentItem, cancellationToken);
            }
        }

        public IEnumerable<ContentItem> GetCollection(string type)
        {
            if (_items.TryGetValue(type, out var collection))
            {
                return collection;
            }

            return Enumerable.Empty<ContentItem>();
        }

        public async IAsyncEnumerable<T> Transform<T>(string type, Func<ContentItem, Task<T>> itemFunc)
        {
            var collection = GetCollection(type);

            foreach (var item in collection)
            {
                yield return await itemFunc(item);
            }
        }
    }
}