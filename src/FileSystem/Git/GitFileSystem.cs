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
            var queue = new Queue<Tree>();
            var gitPath = GetGitPath(directory.Path);

            Tree tree;
            if (string.IsNullOrEmpty(gitPath))
            {
                tree = _branch.Tip.Tree;
            }
            else
            {
                var entry = _branch[gitPath];
                tree = _repo.Lookup<Tree>(entry.Target.Id);
            }
            
            queue.Enqueue(tree);

            do
            {
                tree = queue.Dequeue();

                foreach (var entry in tree)
                {
                    switch (entry.TargetType)
                    {
                        case TreeEntryTargetType.Tree:
                            tree = _repo.Lookup<Tree>(entry.Target.Id);
                            queue.Enqueue(tree);
                            yield return new Directory(this, entry.Path);
                            break;
                        case TreeEntryTargetType.Blob:
                            yield return new File(this, entry.Path);
                            break;
                    }
                }
                
                
            } while (queue.Count > 0);
        }

        private string GetGitPath(in Path path)
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