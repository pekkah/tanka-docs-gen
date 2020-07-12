using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    public class PhysicalFileSystem : IFileSystem
    {
        private readonly string _root;

        public PhysicalFileSystem(string root)
        {
            _root = root;
        }

        public ValueTask<IFile> GetOrCreateFile(Path path)
        {
            var fullPath = GetFullPath(path);
            return new ValueTask<IFile>(new PhysicalFile(
                path,
                fullPath));
        }

        public ValueTask<IDirectory> GetOrCreateDirectory(Path path)
        {
            var fullPath = GetFullPath(path);
            Directory.CreateDirectory(fullPath);

            return new ValueTask<IDirectory>(
                new PhysicalDirectory(
                    this,
                    path,
                    fullPath));
        }

        public ValueTask<IFileSystem> Mount(Path path)
        {
            var fullPath = GetFullPath(path);
            return new ValueTask<IFileSystem>(new PhysicalFileSystem(fullPath));
        }

        public async Task DeleteDir(Path path)
        {
            await Task.Yield();

            var fullPath = GetFullPath(path);

            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, true);
        }

        public async Task CleanDirectory(Path path)
        {
            await Task.Yield();

            var fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
                return;

            foreach (var entry in Directory.EnumerateFiles(fullPath))
            {
                File.Delete(entry);
            }

            foreach (var directory in Directory.EnumerateDirectories(fullPath))
            {
                Directory.Delete(directory, true);
            }
        }

        public async ValueTask<IReadOnlyFile?> GetFile(Path path)
        {
            var fullPath = GetFullPath(path);
            if (!File.Exists(fullPath))
                return null;

            return await GetOrCreateFile(path);
        }

        public async ValueTask<IReadOnlyDirectory?> GetDirectory(Path path)
        {
            var fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
                return null;

            return await GetOrCreateDirectory(path);
        }

        public async IAsyncEnumerable<IFileSystemNode> Enumerate(Path path)
        {
            await Task.Yield();

            var fullPath = GetFullPath(path);
            foreach (var entry in Directory.EnumerateFiles(fullPath))
                yield return new PhysicalFile(
                    GetRelativePath(entry),
                    entry);

            foreach (var entry in Directory.EnumerateDirectories(fullPath))
                yield return new PhysicalDirectory(
                    this,
                    GetRelativePath(entry),
                    entry);
        }

        protected Path GetRelativePath(in Path path)
        {
            return System.IO.Path.GetRelativePath(_root, path);
        }

        protected Path GetFullPath(in Path path)
        {
            if (path == "")
                return _root;

            var fullPath = System.IO.Path.GetFullPath(path, _root);

            return fullPath;
        }
    }
}