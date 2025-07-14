using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tanka.FileSystem
{
    public class PhysicalFileSystem : IFileSystem
    {
        public IFileWatcher? Watcher => new PhysicalFileWatcher();

        private readonly string _root;

        public PhysicalFileSystem(string root)
        {
            _root = root;
        }

        public Task<bool> Exists(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);
            return Task.FromResult(File.Exists(fullPath) || Directory.Exists(fullPath));
        }

        public ValueTask<IReadOnlyFile?> GetFile(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);
            if (!File.Exists(fullPath))
                return new ValueTask<IReadOnlyFile?>();

            return new ValueTask<IReadOnlyFile?>(new PhysicalFile(
                path,
                fullPath));
        }

        async Task<IDirectory?> IFileSystem.GetDirectory(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
                return null;

            return (await GetOrCreateDirectory(path)) as IDirectory;
        }

        public ValueTask<IReadOnlyDirectory?> GetDirectory(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
                return new ValueTask<IReadOnlyDirectory?>();

            return new ValueTask<IReadOnlyDirectory?>(
                new PhysicalDirectory(
                    this,
                    path,
                    fullPath));
        }

        public async IAsyncEnumerable<IFileSystemNode> Enumerate(FileSystemPath path)
        {
            await Task.Yield();

            var fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
                yield break;

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

        public ValueTask<IFile> GetOrCreateFile(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);
            return new ValueTask<IFile>(new PhysicalFile(
                path,
                fullPath));
        }

        public ValueTask<IDirectory> GetOrCreateDirectory(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);
            Directory.CreateDirectory(fullPath);

            return new ValueTask<IDirectory>(
                new PhysicalDirectory(
                    this,
                    path,
                    fullPath));
        }

        public Task DeleteDir(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);

            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, true);

            return Task.CompletedTask;
        }

        public Task DeleteFile(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }

        public Task CleanDirectory(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
                return Task.CompletedTask;

            foreach (var entry in Directory.EnumerateFiles(fullPath))
            {
                File.Delete(entry);
            }

            foreach (var directory in Directory.EnumerateDirectories(fullPath))
            {
                Directory.Delete(directory, true);
            }

            return Task.CompletedTask;
        }

        public ValueTask<IFileSystem> Mount(FileSystemPath path)
        {
            var fullPath = GetFullPath(path);
            return new ValueTask<IFileSystem>(new PhysicalFileSystem(fullPath));
        }

        protected FileSystemPath GetRelativePath(in FileSystemPath path)
        {
            return System.IO.Path.GetRelativePath(_root, path);
        }

        protected FileSystemPath GetFullPath(in FileSystemPath path)
        {
            if (path == "")
                return _root;

            var fullPath = System.IO.Path.GetFullPath(path, _root);

            return fullPath;
        }
    }
}