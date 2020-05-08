using System.IO;
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
            using var fs = new GitFileSystem(RepoRoot, "master");
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
            using var fs = new GitFileSystem(RepoRoot, "master");
            var canEnumerate = false;

            /* When */
            var docsDir = fs.GetDirectory("docs");
            await foreach (var node in docsDir.Enumerate())
            {
                Assert.StartsWith("docs/", node.Path);
                canEnumerate = true;
            }

            /* Then */
            Assert.True(canEnumerate);
        }

        [Fact]
        public void Open_file_for_reading()
        {
            /* Given */
            using var fs = new GitFileSystem(RepoRoot, "master");
            var filename = "README.md";

            /* When */
            var file = fs.GetFile(filename);
            var reader = file.OpenRead();
            using var streamReader = new StreamReader(reader.AsStream());
            var contents = streamReader.ReadToEnd();
            reader.Complete();

            /* Then */
            Assert.NotNull(contents);
        }
    }
}