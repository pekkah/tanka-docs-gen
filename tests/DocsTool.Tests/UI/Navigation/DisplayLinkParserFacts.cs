using Tanka.DocsTool.Navigation;
using Xunit;

namespace Tanka.DocsTool.Tests.Navigation
{
    public class DisplayLinkParserFacts
    {
        public const string ExternalLink = "[title](https://link.invalid)";
        public const string XrefLink = "[title](xref://link.md)";
        public const string XrefWithSectionIdLink = "[title](xref://section:link.md)";

        [Fact]
        public void ExternalLink_ParseTitle()
        {
            /* Given */
            var link = ExternalLink;

            /* When */
            var definition = DisplayLinkParser.Parse(link);

            /* Then */
            Assert.Equal("title", definition.Title);
        }

        [Fact]
        public void XrefLink_ParseTitle()
        {
            /* Given */
            var link = XrefLink;

            /* When */
            var definition = DisplayLinkParser.Parse(link);

            /* Then */
            Assert.Equal("title", definition.Title);
        }

        [Fact]
        public void XrefWithSectionIdLink_ParseTitle()
        {
            /* Given */
            var link = XrefWithSectionIdLink;

            /* When */
            var definition = DisplayLinkParser.Parse(link);

            /* Then */
            Assert.Equal("title", definition.Title);
        }
    }
}