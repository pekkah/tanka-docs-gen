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

    public GitBranchFileSystem Branch(string name)
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
    try
    {
        // Attempt to open the repository directly at startingPath
        // This assumes startingPath is the root of the working directory.
        Console.WriteLine($"Attempting to directly open repository at: {startingPath}");
        var repository = new Repository(startingPath);
        Console.WriteLine($"Successfully opened repository directly at: {startingPath}");
        return repository;
    }
    catch (RepositoryNotFoundException ex)
    {
        Console.WriteLine($"Directly opening repository at {startingPath} failed: {ex.Message}. Falling back to discovery.");
        // Fallback to discovery if direct open fails
        var repoRoot = Repository.Discover(startingPath);
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine($"Repository.Discover also failed for startingPath: {startingPath}");
            throw new InvalidOperationException(
                $"Could not discover Git repository starting from path '{startingPath}'. " +
                $"Both direct open and discovery failed.");
        }

        Console.WriteLine($"Repository.Discover found repository at: {repoRoot}");
        return new Repository(repoRoot);
    }
    catch (Exception ex)
    {
        // Catch any other exceptions during direct open and rethrow as InvalidOperationException
        // to keep behavior somewhat consistent with original if it's not a RepositoryNotFoundException
        Console.WriteLine($"An unexpected error occurred while trying to directly open repository at {startingPath}: {ex.Message}. Discovery will not be attempted.");
            throw new InvalidOperationException(
            $"Failed to open or discover Git repository at '{startingPath}'. Error: {ex.Message}",
            ex);
    }
    }

    public FileSystemPath Path => _repo.Info.Path;

    public static GitFileSystemRoot Discover(string startingPath)
    {
        return new GitFileSystemRoot(DiscoverRepo(startingPath));
    }
}
