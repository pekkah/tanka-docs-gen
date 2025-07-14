using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    public class GitBranchFileSystem : IReadOnlyFileSystem
    {
        public Branch Branch { get; }
        public Repository Repo { get; }

        public string FriendlyName => Branch.FriendlyName;

        public string Sha => Branch.Tip.Sha;

        public string Author => Branch.Tip.Author.Name;

        public GitBranchFileSystem(Repository repository, Branch branch)
        {
            Repo = repository;
            Branch = branch;

            if (Branch == null)
                throw new ArgumentOutOfRangeException(
                    $"Tag '{branch}' does not exists in repository: {Repo.Info.Path}");
        }

        public ValueTask<IReadOnlyFile?> GetFile(FileSystemPath path)
        {
            var entry = Branch[path];
            if (entry.TargetType != TreeEntryTargetType.Blob)
                throw new InvalidOperationException(
                    $"Tree '{entry.Target.Id}' at path '{path}' is not a blob");

            var blob = (Blob)entry.Target;

            return new ValueTask<IReadOnlyFile?>(
                new GitFile(path, entry, blob));
        }

        public async IAsyncEnumerable<IFileSystemNode> Enumerate(FileSystemPath path)
        {
            await Task.Delay(0);
            var queue = new Queue<TreeEntry>();
            var gitPath = GetGitPath(path);

            if (string.IsNullOrEmpty(gitPath))
            {
                foreach (var entry in Branch.Tip.Tree) queue.Enqueue(entry);
            }
            else
            {
                var treeEntry = Branch.Tip.Tree[gitPath];
                var tree = Repo.Lookup<Tree>(treeEntry.Target.Id);

                foreach (var entry in tree) queue.Enqueue(entry);
            }

            do
            {
                var entry = queue.Dequeue();

                switch (entry.TargetType)
                {
                    case TreeEntryTargetType.Tree:
                        var tree = Repo.Lookup<Tree>(entry.Target.Id);
                        yield return new GitDirectory(this, gitPath / entry.Path, tree);
                        break;
                    case TreeEntryTargetType.Blob:
                        yield return new GitFile(
                            gitPath / entry.Path,
                            entry,
                            (Blob)entry.Target);
                        break;
                }
            } while (queue.Count > 0);
        }

        public ValueTask<IReadOnlyDirectory?> GetDirectory(FileSystemPath path)
        {
            var gitPath = GetGitPath(path);

            Tree tree;
            if (!string.IsNullOrEmpty(gitPath))
            {
                var treeEntry = Branch.Tip.Tree[gitPath];

                if (treeEntry == null || treeEntry.TargetType != TreeEntryTargetType.Tree)
                    return new ValueTask<IReadOnlyDirectory?>(default(IReadOnlyDirectory?));

                tree = Repo.Lookup<Tree>(treeEntry.Target.Id);
            }
            else
            {
                tree = Branch.Tip.Tree;
            }

            return new ValueTask<IReadOnlyDirectory?>(
                new GitDirectory(this, gitPath, tree));
        }

        private FileSystemPath GetGitPath(in FileSystemPath path)
        {
            if (path == ".")
                return "";

            return path;
        }
    }
}