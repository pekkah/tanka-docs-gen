using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    public class GitFileSystem: IFileSystem
    {
        private Branch _branch;
        private readonly Repository _repo;
        private Path _absoluteRootPath;

        public GitFileSystem(Path path, string branch)
        {
            _absoluteRootPath = path;
            _repo = new Repository(_absoluteRootPath);
            _branch = _repo.Branches[branch];
            
            Root = new Directory(this, path.GetRelative(_absoluteRootPath));

            if (_branch == null)
                throw new ArgumentOutOfRangeException($"Branch '{branch}' does not exists in repository: {_repo.Info.Path}");
        }

        public Directory Root { get; }

        public async IAsyncEnumerable<IFileSystemNode> EnumerateDirectory(Directory directory)
        {
            await Task.Delay(0);
            var queue = new Queue<TreeEntry>();
            var gitPath = GetGitPath(directory.Path);

            if (string.IsNullOrEmpty(gitPath))
            {
                foreach (var entry in _branch.Tip.Tree)
                {
                    queue.Enqueue(entry);
                }
            }
            else
            {
                TreeEntry treeEntry = _branch.Tip.Tree[gitPath];
                Tree tree = _repo.Lookup<Tree>(treeEntry.Target.Id);

                foreach (var entry in tree)
                {
                    queue.Enqueue(entry);
                }
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

        private Path GetGitPath(in Path path)
        {
            if (path == ".")
                return "";

            return path;
        }

        public IAsyncEnumerable<IFileSystemNode> EnumerateRoot()
        {
            return EnumerateDirectory(Root);
        }

        public PipeReader OpenRead(File file)
        {
            throw new NotImplementedException();
        }

        public PipeWriter OpenWrite(File file)
        {
            throw new NotImplementedException();
        }

        public Directory GetOrCreateDirectory(Path path)
        {
            return new Directory(this, path);
        }
    }
}