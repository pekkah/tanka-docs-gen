using System.Threading.Tasks;
using Tanka.FileSystem.Git;
using Xunit;

namespace Tanka.FileSystem.Tests.Git
{
    public class GitFileSystemFacts
    {
        public GitFileSystemFacts()
        {
            RepoRoot = System.IO.Path.GetFullPath("../../../../../");  
        }

        public string RepoRoot { get; set; }

        [Fact]
        public async Task EnumerateRoot()
        {
            /* Given */
            var fs = new GitFileSystem(RepoRoot, "master");
            var canEnumerate = false;
            
            /* When */
            await foreach (var node in fs.EnumerateRoot())
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
            var fs = new GitFileSystem(RepoRoot, "master");
            var canEnumerate = false;

            /* When */
            var docsDir = fs.GetOrCreateDirectory("docs");
            await foreach (var node in docsDir.Enumerate())
            {
                Assert.StartsWith("docs/", node.Path);
                canEnumerate = true;
            }

            /* Then */
            Assert.True(canEnumerate);
        }
    }
}