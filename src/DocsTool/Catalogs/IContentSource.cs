using System.Collections.Generic;
using System.Threading;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
{
    public interface IContentSource
    {
        public string Version { get; }

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken);
    }
}