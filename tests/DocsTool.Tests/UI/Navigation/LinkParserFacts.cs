using System.Text;
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
        public const string XrefWithSectionIdAndVersionAndQuery = "xref://section@release/1.0.0:link.md?a=1&b=2&c=3";

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

        [Fact]
        public void ParseXref_with_SectionIdAndVersionAndQuery1()
        {
            /* Given */
            var link = XrefWithSectionIdAndVersionAndQuery;

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal("link.md", definition.Xref?.Path);
            Assert.Equal("section", definition.Xref?.SectionId);
            Assert.Equal("release/1.0.0", definition.Xref?.Version);
            Assert.Equal(3, definition.Xref?.Query.Count);
        }

        [Fact]
        public void ParseXref_with_SectionIdAndVersionAndQuery2()
        {
            /* Given */
            var link = "xref://section@release/1.0.0:link.md?a=1";

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal("link.md", definition.Xref?.Path);
            Assert.Equal("section", definition.Xref?.SectionId);
            Assert.Equal("release/1.0.0", definition.Xref?.Version);
            Assert.Single(definition.Xref?.Query, kv => kv.Key == "a" && kv.Value == "1");
            Assert.Equal(1, definition.Xref?.Query.Count);
        }

        [Fact]
        public void ParseXref_with_SectionIdAndVersionAndQuery3()
        {
            /* Given */
            var link = "xref://section@release/1.0.0:link.md?abc=123_321";

            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal("link.md", definition.Xref?.Path);
            Assert.Equal("section", definition.Xref?.SectionId);
            Assert.Equal("release/1.0.0", definition.Xref?.Version);
            Assert.Single(definition.Xref?.Query, kv => kv.Key == "abc" && kv.Value == "123_321");
            Assert.Equal(1, definition.Xref?.Query.Count);
        }

        [Fact]
        public void ParseXref_from_bytes()
        {
            /* Given */
            var bytes = Encoding.UTF8.GetBytes("xref://section@release/1.0.0:link.md?abc=123_321");

            /* When */
            var definition = LinkParser.Parse(bytes);

            /* Then */
            Assert.True(definition.IsXref);
            Assert.False(definition.IsExternal);
            Assert.Equal("link.md", definition.Xref?.Path);
            Assert.Equal("section", definition.Xref?.SectionId);
            Assert.Equal("release/1.0.0", definition.Xref?.Version);
            Assert.Single(definition.Xref?.Query, kv => kv.Key == "abc" && kv.Value == "123_321");
            Assert.Equal(1, definition.Xref?.Query.Count);
        }
    }
}