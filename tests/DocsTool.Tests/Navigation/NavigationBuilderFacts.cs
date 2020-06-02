using Markdig.Parsers;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Xunit;

namespace Tanka.DocsTool.Tests.Navigation
{
    public class NavigationBuilderFacts
    {
        private NavigationBuilder _sut;

        public NavigationBuilderFacts()
        {
            _sut = new NavigationBuilder();
        }

        [Fact]
        public void Build_from_ListBlock()
        {
            /* Given */
            var document = @"
* [label1](https://link.to)
* [label2](xref://section:page.md)
".ParseMarkdown();

            /* When */
            var items = _sut.Add(document).Build();

            /* Then */
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void Build_from_ListBlock_with_sub_items()
        {
            /* Given */
            var document = @"
* [label1](https://link.to)
    * [sublabel1](https://link.to.1)
    * [sublabel2](xref://section:page.md)
".ParseMarkdown();

            /* When */
            var items = _sut.Add(document).Build();

            /* Then */
            var parent = Assert.Single(items);
            Assert.Equal(2, parent.Children.Count);
        }
    }
}