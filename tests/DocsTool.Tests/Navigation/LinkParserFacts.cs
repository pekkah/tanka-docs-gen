using Tanka.DocsTool.Navigation;
using Xunit;

namespace Tanka.DocsTool.Tests.Navigation
{
    public class LinkParserFacts
    {
        public const string ExternalLink = "https://link.invalid";
        public const string XrefLink = "xref://link.md";
        public const string XrefWithSectionIdLink = "xref://section:link.md";
        public const string XrefWithSectionIdAndVersionLink = "xref://section@release/1.0.0:link.md";

        [Fact]
        public void ParseUri()
        {
            /* Given */
            var link = ExternalLink;

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsExternal);
            Assert.False(definition.IsXref);
            Assert.Equal("https://link.invalid", definition.Uri);
        }

        [Fact]
        public void ParseXref()
        {
            /* Given */
            var link = XrefLink;

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal("link.md", definition.Xref?.Path);
            Assert.Null(definition.Xref?.SectionId);
        }

        [Fact]
        public void ParseXref_with_SectionId()
        {
            /* Given */
            var link = XrefWithSectionIdLink;

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal("link.md", definition.Xref?.Path);
            Assert.Equal("section", definition.Xref?.SectionId);
        }

        [Fact]
        public void ParseXref_with_SectionIdAndVersion()
        {
            /* Given */
            var link = XrefWithSectionIdAndVersionLink;

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal("link.md", definition.Xref?.Path);
            Assert.Equal("section", definition.Xref?.SectionId);
            Assert.Equal("release/1.0.0", definition.Xref?.Version);
        }
    }
}