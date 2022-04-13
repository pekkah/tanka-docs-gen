using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Catalogs
{
    public class GitBranchContentSource : IContentSource
    {
        private readonly GitBranchFileSystem _branch;
        private readonly FileSystemPath _path;

        public GitBranchContentSource(GitBranchFileSystem branch, FileSystemPath path)
        {
            _branch = branch;
            _path = path;
        }

        public string Version => _branch.FriendlyName;

        public FileSystemPath Path => _path;

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken) => _branch.Enumerate(_path);
    }

    public class GitCommitContentSource : IContentSource
    {
        private readonly GitCommitFileSystem _commit;
        private readonly string _friendlyName;
        private readonly FileSystemPath _path;

        public GitCommitContentSource(GitCommitFileSystem commit, string friendlyName, FileSystemPath path)
        {
            _commit = commit;
            _friendlyName = friendlyName;
            _path = path;
        }

        public string Version => _friendlyName;

        public FileSystemPath Path => _path;

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken) => _commit.Enumerate(_path);
    }
}