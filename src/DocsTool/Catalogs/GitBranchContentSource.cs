using System.Collections.Generic;
using System.Threading;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;

namespace Tanka.DocsTool.Catalogs
{
    public class GitBranchContentSource : IContentSource
    {
        private readonly GitBranchFileSystem _branch;
        private readonly Path _path;

        public GitBranchContentSource(GitBranchFileSystem branch, Path path)
        {
            _branch = branch;
            _path = path;
        }

        public string Version => _branch.FriendlyName;

        public Path Path => _path;

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken)
        {
            return _branch.Enumerate(_path);
        }
    }

    public class GitCommitContentSource : IContentSource
    {
        private readonly GitCommitFileSystem _commit;
        private readonly string _friendlyName;
        private readonly Path _path;

        public GitCommitContentSource(GitCommitFileSystem commit, string friendlyName, Path path)
        {
            _commit = commit;
            _friendlyName = friendlyName;
            _path = path;
        }

        public string Version => _friendlyName;

        public Path Path => _path;

        public IAsyncEnumerable<IFileSystemNode> Enumerate(CancellationToken cancellationToken)
        {
            return _commit.Enumerate(_path);
        }
    }
}