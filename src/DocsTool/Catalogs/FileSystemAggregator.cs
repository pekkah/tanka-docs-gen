using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Globbing;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public class FileSystemAggregator
    {
        private readonly IFileSystem _fileSystem;

        public FileSystemAggregator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async IAsyncEnumerable<IReadOnlyFile> Aggregate(
            Path path,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var stack = new Stack<IReadOnlyDirectory>();
            var root = await _fileSystem.GetDirectory(path);

            if (root == null)
                yield break;

            stack.Push(root);

            do
            {
                var dir = stack.Pop();

                await foreach (var node in dir.Enumerate().WithCancellation(cancellationToken))
                {
                    switch (node)
                    {
                        case IReadOnlyFile file:
                            yield return file;
                            break;
                        case IReadOnlyDirectory subDir:
                            stack.Push(subDir);
                            break;
                    }
                }
            } while (stack.Count > 0);
        }

        public async IAsyncEnumerable<IReadOnlyFile> Aggregate(
            Path path,
            IEnumerable<Path> filePatterns,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var stack = new Stack<IReadOnlyDirectory>();
            var root = await _fileSystem.GetDirectory(path);

            if (root == null)
                yield break;

            var globs = filePatterns.Select(p => Glob.Parse(p))
                .ToList();

            stack.Push(root);

            do
            {
                var dir = stack.Pop();

                await foreach (var node in dir.Enumerate().WithCancellation(cancellationToken))
                {
                    switch (node)
                    {
                        case IReadOnlyFile file:
                            if (globs.Any(g => g.IsMatch(file.Path)))
                                yield return file;
                            break;
                        case IReadOnlyDirectory subDir:
                            stack.Push(subDir);
                            break;
                    }
                }
            } while (stack.Count > 0);
        }
    }
}