using System.Collections.Concurrent;
using System.Collections.Generic;
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
    }
}