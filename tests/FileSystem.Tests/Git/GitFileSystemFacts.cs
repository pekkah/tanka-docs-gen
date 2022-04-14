using System;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Tanka.FileSystem.Git;
using Xunit;

namespace Tanka.FileSystem.Tests.Git
{
    public class GitFileSystemFacts: IDisposable
    {
        public readonly GitFileSystemRoot RootFs;

        public GitFileSystemFacts()
        {
            RepoRoot = Repository.Discover(Environment.CurrentDirectory);
            Repo = new Repository(RepoRoot);
            RootFs = new GitFileSystemRoot(Repo);
        }

        public Repository Repo { get; set; }

        public string RepoRoot { get; set; }

        [Fact]
        public async Task EnumerateRoot()
        {
            /* Given */
            var fs = RootFs.Head();
            var canEnumerate = false;
            
            /* When */
            await foreach (var node in fs.Enumerate(""))
            {
                canEnumerate = true;
            }

            /* Then */
            Assert.True(canEnumerate);
        }

        [Fact]
        public async Task Enumerate_known_folder()
        {
            /* Given */
            var fs = RootFs.Head();
            var canEnumerate = false;

            /* When */
            var docsDir = await fs.GetDirectory("docs-v2");
            await foreach (var node in docsDir!.Enumerate())
            {
                Assert.StartsWith("docs-v2/", node.Path);
                canEnumerate = true;
            }

            /* Then */
            Assert.True(canEnumerate);
        }

        [Fact]
        public async Task Open_file_for_reading()
        {
            /* Given */
            var fs = RootFs.Head();
            var filename = "README.md";

            /* When */
            var file = await fs.GetFile(filename);
            using var streamReader = new StreamReader(await file.OpenRead());
            var contents = await streamReader.ReadToEndAsync();

            /* Then */
            Assert.NotNull(contents);
        }

        public void Dispose()
        {
            Repo.Dispose();
        }
    }
}