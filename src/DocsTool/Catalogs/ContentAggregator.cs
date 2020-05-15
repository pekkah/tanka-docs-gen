using System.Collections.Generic;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public interface IContentClassifier
    {
        string Classify(Directory directory, File file);
    }

    public class ContentAggregator
    {
        private readonly IContentClassifier _contentClassifier;
        private readonly IReadOnlyFileSystem[] _fileSystems;

        public ContentAggregator(
            IContentClassifier contentClassifier,
            params IReadOnlyFileSystem[] fileSystems)
        {
            _contentClassifier = contentClassifier;
            _fileSystems = fileSystems;
        }

        public async IAsyncEnumerable<ContentItem> Enumerate()
        {
            var queue = new Queue<Directory>();

            foreach (var fileSystem in _fileSystems) queue.Enqueue(fileSystem.GetDirectory("."));

            while (queue.Count > 0)
            {
                var directory = queue.Dequeue();

                await foreach (var node in directory.Enumerate())
                    switch (node)
                    {
                        case Directory subDirectory:
                            queue.Enqueue(subDirectory);
                            break;
                        case File file:
                            yield return CreateContentItem(directory, file);
                            break;
                    }
            }
        }

        private ContentItem CreateContentItem(Directory directory, File file)
        {
            var type = _contentClassifier.Classify(directory, file);
            return new ContentItem(directory, file, type);
        }
    }
}