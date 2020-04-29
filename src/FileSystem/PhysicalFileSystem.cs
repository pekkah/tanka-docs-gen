using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    public class PhysicalFileSystem : IFileSystem
    {
        public PhysicalFileSystem(Path root)
        {
            Root = new Directory(this, root);
        }

        public Directory Root { get; }

        public async IAsyncEnumerable<IFileSystemNode> EnumerateDirectory(Directory directory)
        {
            await Task.Yield();
            var path = GetFullPath(directory.Path);
            foreach (var entry in System.IO.Directory.EnumerateFiles(path))
                yield return new File(this, entry);

            foreach (var entry in System.IO.Directory.EnumerateDirectories(path))
                yield return new Directory(this, entry);
        }

        public IAsyncEnumerable<IFileSystemNode> EnumerateRoot()
        {
            return EnumerateDirectory(Root);
        }

        public PipeReader OpenRead(File file)
        {
            var fileStream = System.IO.File.OpenRead(GetFullPath(file.Path));
            return PipeReader.Create(fileStream);
        }

        public PipeWriter OpenWrite(File file)
        {
            var fileStream = System.IO.File.OpenWrite(GetFullPath(file.Path));
            return PipeWriter.Create(fileStream);
        }

        public Directory GetOrCreateDirectory(Path path)
        {
            var fullPath = GetFullPath(path);
            System.IO.Directory.CreateDirectory(fullPath);
            return new Directory(this, path);
        }

        protected Path GetFullPath(Path path)
        {
            return Root.Path / path;
        }
    }
}