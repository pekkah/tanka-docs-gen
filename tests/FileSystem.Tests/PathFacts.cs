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
            Path path = "seg1\\seg2\\seg3";

            /* When */
            Assert.Equal("seg1/seg2/seg3", path);
        }

        [Fact]
        public void Normalize_mixed_path()
        {
            /* Given */
            /* When */
            Path path = "seg1/seg2\\seg3";

            /* When */
            Assert.Equal("seg1/seg2/seg3", path);
        }

        [Fact]
        public void Do_not_strip_dot_from_filepath()
        {
            /* Given */
            /* When */
            Path path = ".gitattributes";

            /* When */
            Assert.Equal(".gitattributes", path);
        }

        [Fact]
        public void Strip_dot_from_dirpath()
        {
            /* Given */
            /* When */
            Path path = "./files";

            /* When */
            Assert.Equal("files", path);
        }

        [Fact]
        public void Strip_slash_from_dirpath()
        {
            /* Given */
            /* When */
            Path path = "/files";

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
            Path left = leftStr;
            Path right = rightStr;

            /* When */
            Path path = left - right;

            /* Then */
            Assert.Equal(expectedStr, path);

        }
    }
}