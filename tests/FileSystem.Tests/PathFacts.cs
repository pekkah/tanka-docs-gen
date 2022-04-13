using Xunit;

namespace Tanka.FileSystem.Tests
{
    public class PathFacts
    {
        [Fact]
        public void Normalize_path()
        {
            /* Given */
            /* When */
            FileSystemPath path = "seg1\\seg2\\seg3";

            /* When */
            Assert.Equal("seg1/seg2/seg3", path);
        }

        [Fact]
        public void Normalize_mixed_path()
        {
            /* Given */
            /* When */
            FileSystemPath path = "seg1/seg2\\seg3";

            /* When */
            Assert.Equal("seg1/seg2/seg3", path);
        }

        [Fact]
        public void Do_not_strip_dot_from_filepath()
        {
            /* Given */
            /* When */
            FileSystemPath path = ".gitattributes";

            /* When */
            Assert.Equal(".gitattributes", path);
        }

        [Fact]
        public void Strip_dot_from_dirpath()
        {
            /* Given */
            /* When */
            FileSystemPath path = "./files";

            /* When */
            Assert.Equal("files", path);
        }

        [Fact]
        public void Strip_slash_from_dirpath()
        {
            /* Given */
            /* When */
            FileSystemPath path = "/files";

            /* When */
            Assert.Equal("files", path);
        }

        [Theory]
        [InlineData("seg1/seg2/seg3/file.txt", "seg1/seg2/seg3", "file.txt")]
        [InlineData("seg1/seg2/seg3", "seg1/seg2/seg3", "")]
        [InlineData("seg1", "seg1", "")]
        [InlineData("seg1/seg2", "seg3", "seg1/seg2")]
        [InlineData("file.txt", "file.txt", "")]
        [InlineData("seg1/seg2/seg3", "seg1/seg2/seg3/seg4", "")]
        public void Subtract_path(string leftStr, string rightStr, string expectedStr)
        {
            /* Given */
            FileSystemPath left = leftStr;
            FileSystemPath right = rightStr;

            /* When */
            FileSystemPath path = left - right;

            /* Then */
            Assert.Equal(expectedStr, path);

        }
    }
}