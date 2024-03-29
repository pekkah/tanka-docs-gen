﻿using System.Collections.Generic;
using LibGit2Sharp;

namespace Tanka.FileSystem.Git
{
    internal class GitDirectory : IReadOnlyDirectory
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public GitDirectory(IReadOnlyFileSystem fileSystem, in FileSystemPath path, Tree tree)
        {
            Path = path;
            Tree = tree;
            _fileSystem = fileSystem;
        }

        public Tree Tree { get; }

        public FileSystemPath Path { get; }

        public IAsyncEnumerable<IFileSystemNode> Enumerate()
        {
            return _fileSystem.Enumerate(Path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}