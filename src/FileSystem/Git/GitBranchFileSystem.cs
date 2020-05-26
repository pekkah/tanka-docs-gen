using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    public class GitBranchFileSystem : IReadOnlyFileSystem
    {
        private readonly Branch _branch;
        private readonly Repository _repo;

        public GitBranchFileSystem(Repository repository, Branch branch)
        {
            _repo = repository;
            _branch = branch;

            if (_branch == null)
                throw new ArgumentOutOfRangeException(
                    $"Branch '{branch}' does not exists in repository: {_repo.Info.Path}");
        }

        public ValueTask<IReadOnlyFile?> GetFile(Path path)
        {
            var entry = _branch[path];
            if (entry.TargetType != TreeEntryTargetType.Blob)
                throw new InvalidOperationException(
                    $"Tree '{entry.Target.Id}' at path '{path}' is not a blob");

            var blob = (Blob) entry.Target;

            return new ValueTask<IReadOnlyFile?>(
                new GitFile(path, entry, blob));
        }

        public async IAsyncEnumerable<IFileSystemNode> Enumerate(Path path)
        {
            await Task.Delay(0);
            var queue = new Queue<TreeEntry>();
            var gitPath = GetGitPath(path);

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
                        var tree = _repo.Lookup<Tree>(entry.Target.Id);
                        yield return new GitDirectory(this, gitPath / entry.Path, tree);
                        break;
                    case TreeEntryTargetType.Blob:
                        yield return new GitFile(
                            gitPath / entry.Path,
                            entry,
                            (Blob) entry.Target);
                        break;
                }
            } while (queue.Count > 0);
        }

        public ValueTask<IReadOnlyDirectory?> GetDirectory(Path path)
        {
            var gitPath = GetGitPath(path);

            Tree tree;
            if (!string.IsNullOrEmpty(gitPath))
            {
                var treeEntry = _branch.Tip.Tree[gitPath];
                tree = _repo.Lookup<Tree>(treeEntry.Target.Id);

                if (treeEntry == null || treeEntry.TargetType != TreeEntryTargetType.Tree)
                    return new ValueTask<IReadOnlyDirectory?>(default(IReadOnlyDirectory?));
            }
            else
            {
                tree = _branch.Tip.Tree;
            }

            return new ValueTask<IReadOnlyDirectory?>(
                new GitDirectory(this, gitPath, tree));
        }

        private Path GetGitPath(in Path path)
        {
            if (path == ".")
                return "";

            return path;
        }
    }

    internal class GitDirectory : IReadOnlyDirectory
    {
        private readonly GitBranchFileSystem _fileSystem;

        public GitDirectory(GitBranchFileSystem fileSystem, in Path path, Tree tree)
        {
            Path = path;
            Tree = tree;
            _fileSystem = fileSystem;
        }

        public Tree Tree { get; }

        public Path Path { get; }

        public IAsyncEnumerable<IFileSystemNode> Enumerate()
        {
            return _fileSystem.Enumerate(Path);
        }
    }

    internal class GitFile : IReadOnlyFile
    {
        private readonly Blob _blob;

        public GitFile(in Path path, TreeEntry entry, Blob blob)
        {
            _blob = blob;
            Path = path;
            Entry = entry;
        }

        public TreeEntry Entry { get; }

        public Path Path { get; }

        public ValueTask<Stream> OpenRead()
        {
            return new ValueTask<Stream>(_blob.GetContentStream());
        }
    }
}