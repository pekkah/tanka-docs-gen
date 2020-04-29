using System.IO.Pipelines;

namespace Tanka.FileSystem
{
    public class File : IFileSystemNode
    {
        private readonly IFileSystem _fileSystem;

        public File(IFileSystem fileSystem, Path path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public Path Path { get; }

        public PipeReader OpenRead()
        {
            return _fileSystem.OpenRead(this);
        }

        public PipeWriter OpenWrite()
        {
            return _fileSystem.OpenWrite(this);
        }
    }
}