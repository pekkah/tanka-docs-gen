using System.Threading.Tasks;
using Tanka.FileSystem.Git;
using Xunit;

namespace Tanka.FileSystem.Tests.Git
{
    public class GitFileSystemFacts
    {
        public GitFileSystemFacts()
        {
            Root = System.IO.Path.GetFullPath("../../../../../");  
        }

        public string Root { get; set; }

        [Fact]
        public async Task EnumerateRoot()
        {
            /* Given */
            var fs = new GitFileSystem(Root, "master");
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
            var fs = new GitFileSystem(Root, "master");
            var canEnumerate = false;

            /* When */
            var docsDir = fs.GetOrCreateDirectory("docs");
            await foreach (var node in docsDir.Enumerate())
            {
                canEnumerate = true;
            }

            /* Then */
            Assert.True(canEnumerate);
        }
    }
}