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
        private readonly IEnumerable<Directory> _directories;

        public ContentAggregator(
            IContentClassifier contentClassifier,
            IEnumerable<Directory> directories)
        {
            _contentClassifier = contentClassifier;
            _directories = directories;
        }

        public ContentAggregator(
            IContentClassifier contentClassifier,
            params Directory[] directories)
        {
            _contentClassifier = contentClassifier;
            _directories = directories;
        }

        public async IAsyncEnumerable<ContentItem> Enumerate()
        {
            var queue = new Queue<Directory>();

            foreach (var fileSystem in _directories) 
                queue.Enqueue(fileSystem);

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