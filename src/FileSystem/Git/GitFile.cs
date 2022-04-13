using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    internal class GitFile : IReadOnlyFile
    {
        private readonly Blob _blob;

        public GitFile(in FileSystemPath path, TreeEntry entry, Blob blob)
        {
            _blob = blob;
            Path = path;
            Entry = entry;
        }

        public TreeEntry Entry { get; }

        public FileSystemPath Path { get; }

        public ValueTask<Stream> OpenRead()
        {
            return new ValueTask<Stream>(_blob.GetContentStream());
        }

        public override string ToString()
        {
            return Path;
        }
    }
}