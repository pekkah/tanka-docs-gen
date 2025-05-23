using LibGit2Sharp;

namespace Tanka.FileSystem.Git;

public class GitFileSystemRoot: GitBranchFileSystem
{
    private readonly Repository _repo;

    public GitFileSystemRoot(Repository repository)
        :base(repository, repository.Head)
    {
        _repo = repository;
    }

    public new GitBranchFileSystem Branch(string name)
    {
        return new GitBranchFileSystem(_repo, _repo.Branches[name]);
    }

    public GitCommitFileSystem Tag(Tag tag)
    {
        var commit = _repo.Lookup<Commit>(tag.Target.Id);
        return new GitCommitFileSystem(_repo, commit);
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

    public FileSystemPath Path => _repo.Info.Path;

    public static GitFileSystemRoot Discover(string startingPath)
    {
        return new GitFileSystemRoot(DiscoverRepo(startingPath));
    }
}
