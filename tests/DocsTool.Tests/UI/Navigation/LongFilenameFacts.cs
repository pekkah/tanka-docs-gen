using Tanka.DocsTool.Navigation;
using Xunit;

namespace Tanka.DocsTool.Tests.Navigation
{
    public class LongFilenameFacts
    {
        [Fact]
        public void LinkParser_ShouldParseLongFilename()
        {
            /* Given */
            var longFilename = "Chibi.Ui.Benchmarks.Graphics.Basic.BasicDrawingBenchmarks-report-github.md";
            var link = $"xref://{longFilename}";

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal(longFilename, definition.Xref?.Path);
            Assert.Null(definition.Xref?.SectionId);
        }

        [Fact]
        public void LinkParser_ShouldParseLongFilenameWithSection()
        {
            /* Given */
            var longFilename = "Chibi.Ui.Benchmarks.Graphics.Basic.BasicDrawingBenchmarks-report-github.md";
            var link = $"xref://benchmarks:{longFilename}";

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal(longFilename, definition.Xref?.Path);
            Assert.Equal("benchmarks", definition.Xref?.SectionId);
        }

        [Theory]
        [InlineData("Very.Long.Namespace.Class.Method.Benchmark-Results-Report.md")]
        [InlineData("Another.Really.Long.ClassName.WithManySegments.AndNumbers123.AndMore-final-report.md")]
        [InlineData("Extremely.Long.File.Name.With.Multiple.Dots.And.Dashes.And.Numbers.123.456.789.Final.md")]
        public void LinkParser_ShouldHandleVaryingLongFilenames(string filename)
        {
            /* Given */
            var link = $"xref://{filename}";

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.Equal(filename, definition.Xref?.Path);
        }
    }
}