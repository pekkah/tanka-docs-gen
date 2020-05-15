using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    public class GitFileSystem : IReadOnlyFileSystem, IDisposable
    {
        private readonly Repository _repo;
        private readonly Branch _branch;

        public GitFileSystem(Path path, string branch)
        {
            _repo = new Repository(path);
            _branch = _repo.Branches[branch];

            if (_branch == null)
                throw new ArgumentOutOfRangeException(
                    $"Branch '{branch}' does not exists in repository: {_repo.Info.Path}");
        }

        public void Dispose()
        {
            _repo?.Dispose();
        }

        public async IAsyncEnumerable<IFileSystemNode> EnumerateDirectory(Directory directory)
        {
            await Task.Delay(0);
            var queue = new Queue<TreeEntry>();
            var gitPath = GetGitPath(directory.Path);

            if (string.IsNullOrEmpty(gitPath))
            {
                foreach (var entry in _branch.Tip.Tree) queue.Enqueue(entry);
            }
            else
            {
                var treeEntry = _branch.Tip.Tree[gitPath];
                var tree = _repo.Lookup<Tree>(treeEntry.Target.Id);

                foreach (var entry in tree) queue.Enqueue(entry);
            }

            do
            {
                var entry = queue.Dequeue();

                switch (entry.TargetType)
                {
                    case TreeEntryTargetType.Tree:
                        yield return new Directory(this, gitPath / entry.Path);
                        break;
                    case TreeEntryTargetType.Blob:
                        yield return new File(this, gitPath / entry.Path);
                        break;
                }
            } while (queue.Count > 0);
        }

        public IAsyncEnumerable<IFileSystemNode> EnumerateRoot()
        {
            return EnumerateDirectory(GetDirectory(""));
        }

        public PipeReader OpenRead(File file)
        {
            var entry = _branch[file.Path];
            if (entry.TargetType != TreeEntryTargetType.Blob)
                throw new InvalidOperationException(
                    $"Entry '{entry.Target.Id}' at path '{file}' is not a blob");

            var blob = (Blob) entry.Target;
            var contentStream = blob.GetContentStream();

            return PipeReader.Create(contentStream);
        }

        public PipeWriter OpenWrite(File file)
        {
            throw new NotImplementedException();
        }

        public Directory GetDirectory(Path path)
        {
            return new Directory(this, GetGitPath(path));
        }

        private Path GetGitPath(in Path path)
        {
            if (path == ".")
                return "";

            return path;
        }

        public File GetFile(Path path)
        {
            return new File(this, GetGitPath(path));
        }
    }
}