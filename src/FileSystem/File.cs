using System.IO.Pipelines;

namespace Tanka.FileSystem
{
    public class File : IFileSystemNode
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public File(IReadOnlyFileSystem fileSystem, Path path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public Path Path { get; }

        public PipeReader OpenRead()
        {
            return _fileSystem.OpenRead(this);
        }

        public override string ToString()
        {
            return Path.ToString();
        }
    }
}