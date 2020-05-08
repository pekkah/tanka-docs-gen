using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    public class PhysicalFileSystem : IFileSystem
    {
        public PhysicalFileSystem(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public async IAsyncEnumerable<IFileSystemNode> EnumerateDirectory(Directory directory)
        {
            await Task.Yield();
            var path = GetFullPath(directory.Path);
            foreach (var entry in System.IO.Directory.EnumerateFiles(path))
                yield return new File(this, System.IO.Path.GetRelativePath(Root, entry));

            foreach (var entry in System.IO.Directory.EnumerateDirectories(path))
                yield return new Directory(this, System.IO.Path.GetRelativePath(Root, entry));
        }

        public IAsyncEnumerable<IFileSystemNode> EnumerateRoot()
        {
            return EnumerateDirectory(new Directory(this, "."));
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

        public Directory GetDirectory(Path path)
        {
            return new Directory(this, path);
        }

        public File GetFile(Path path)
        {
            return new File(this, path);
        }

        protected Path GetFullPath(Path path)
        {
            return System.IO.Path.GetFullPath(path, Root);
        }
    }
}