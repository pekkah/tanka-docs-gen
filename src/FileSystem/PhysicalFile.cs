using System.IO;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    internal class PhysicalFile : IFile
    {
        private readonly string _fullPath;

        public PhysicalFile(Path path, string fullPath)
        {
            Path = path;
            _fullPath = fullPath;
        }

        public Path Path { get; }

        public ValueTask<Stream> OpenRead()
        {
            var stream = File.OpenRead(_fullPath);
            return new ValueTask<Stream>(stream);
        }

        public ValueTask<Stream> OpenWrite()
        {
            var stream = File.OpenWrite(_fullPath);
            return new ValueTask<Stream>(stream);
        }
    }
}