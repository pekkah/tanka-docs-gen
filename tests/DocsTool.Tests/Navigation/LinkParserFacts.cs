﻿using Tanka.DocsTool.Navigation;
using Xunit;

namespace Tanka.DocsTool.Tests.Navigation
{
    public class LinkParserFacts
    {
        public const string ExternalLink = "[title](https://link.invalid)";
        public const string XrefLink = "[title](xref://link.md)";
        public const string XrefWithSectionIdLink = "[title](xref://section:link.md)";

        [Fact]
        public void ParseTitle()
        {
            /* Given */
            var link = ExternalLink;
            
            /* When */
            var definition = LinkParser.Parse(link);

            /* Then */
            Assert.Equal("title", definition.Title);
        }

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
    }
}