using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    public class GitFileSystem : IReadOnlyFileSystem
    {
        private readonly Repository _repo;

        public GitFileSystem(Repository repository)
        {
            _repo = repository;
        }

        public GitBranchFileSystem Branch(string name)
        {
            return new GitBranchFileSystem(_repo, name);
        }

        public async IAsyncEnumerable<IFileSystemNode> EnumerateDirectory(Directory directory)
        {
            await Task.Delay(0);
            var queue = new Queue<TreeEntry>();
            var gitPath = GetGitPath(directory.Path);

            if (string.IsNullOrEmpty(gitPath))
            {
                foreach (var entry in _repo.Head.Tip.Tree) queue.Enqueue(entry);
            }
            else
            {
                var treeEntry = _repo.Head.Tip.Tree[gitPath];
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
            var entry = _repo.Head[file.Path];
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

        public static Repository DiscoverRepository(string startingPath)
        {
            var repoRoot = Repository.Discover(startingPath);
            if (string.IsNullOrEmpty(repoRoot))
                throw new InvalidOperationException(
                    $"Could not discover Git repository starting from " +
                    $"path '{startingPath}'.");

            return new Repository(repoRoot);
        }
    }

    public class GitBranchFileSystem : IReadOnlyFileSystem
    {
        private readonly Repository _repo;
        private readonly Branch _branch;

        public GitBranchFileSystem(Repository repository, string branch)
        {
            _repo = repository;
            _branch = _repo.Branches[branch];

            if (_branch == null)
                throw new ArgumentOutOfRangeException(
                    $"Branch '{branch}' does not exists in repository: {_repo.Info.Path}");
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

            var blob = (Blob)entry.Target;
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

        public static Repository Discover(string startingPath)
        {
            var repoRoot = Repository.Discover(startingPath);
            if (string.IsNullOrEmpty(repoRoot))
                throw new InvalidOperationException(
                    $"Could not discover Git repository starting from " +
                    $"path '{startingPath}'.");

            return new Repository(repoRoot);
        }
    }
}