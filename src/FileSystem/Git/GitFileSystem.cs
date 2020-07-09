using System;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    public class GitFileSystemRoot: GitBranchFileSystem
    {
        private readonly Repository _repo;

        public GitFileSystemRoot(Repository repository)
            :base(repository, repository.Head)
        {
            _repo = repository;
        }

        public GitBranchFileSystem Branch(string name)
        {
            return new GitBranchFileSystem(_repo, _repo.Branches[name]);
        }

        public GitBranchFileSystem Tag(Tag tag)
        {
            return new GitBranchFileSystem(_repo, _repo.Branches[tag.CanonicalName]);
        }

        public GitBranchFileSystem Head()
        {
            return new GitBranchFileSystem(_repo, _repo.Head);
        }

        public static Repository DiscoverRepo(string startingPath)
        {
            var repoRoot = Repository.Discover(startingPath);
            if (string.IsNullOrEmpty(repoRoot))
                throw new InvalidOperationException(
                    $"Could not discover Git repository starting from " +
                    $"path '{startingPath}'.");

            return new Repository(repoRoot);
        }

        public static GitFileSystemRoot Discover(string startingPath)
        {
            return new GitFileSystemRoot(DiscoverRepo(startingPath));
        }
    }
}