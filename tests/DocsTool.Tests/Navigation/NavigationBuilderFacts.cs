using Tanka.DocsTool.Navigation;
using Xunit;

namespace Tanka.DocsTool.Tests.Navigation
{
    public class NavigationBuilderFacts
    {
        public NavigationBuilderFacts()
        {
            _sut = new NavigationBuilder();
        }

        private readonly NavigationBuilder _sut;

        [Fact]
        public void Add_DisplayLink()
        {
            /* Given */
            var link = "[title](xref://nav.md)";


            /* When */
            var linkDefinitions = _sut
                .AddLink(link)
                .Build();

            /* Then */
            var definition = Assert.Single(linkDefinitions);
            Assert.Equal("title", definition.Title);
            Assert.True(definition.Link.IsXref);
        }

        [Fact]
        public void Add_DisplayLinks()
        {
            /* Given */
            var links = new[]
            {
                "[title](xref://nav.md)"
            };


            /* When */
            var linkDefinitions = _sut
                .AddLinks(links)
                .Build();

            /* Then */
            var definition = Assert.Single(linkDefinitions);
            Assert.Equal("title", definition.Title);
            Assert.True(definition.Link.IsXref);
        }
    }
}