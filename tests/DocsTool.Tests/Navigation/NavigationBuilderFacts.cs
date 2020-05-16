using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Tanka.DocsTool.Navigation;
using Tanka.FileSystem;
using Xunit;

namespace Tanka.DocsTool.Tests.Navigation
{
    public class NavigationBuilderFacts
    {
        private IReadOnlyFileSystem _fileSystem;
        private NavigationBuilder _sut;

        public NavigationBuilderFacts()
        {
            _fileSystem = Substitute.For<IReadOnlyFileSystem>();
            _sut = new NavigationBuilder();
        }

        [Fact]
        public void Add_Links()
        {
            /* Given */
            var links = new string[]
            {
                "[title](xref://nav.md)"
            };


            /* When */
            var linkDefinitions = _sut
                .AddLinks(links)
                .Build();

            /* Then */
            var definition  = Assert.Single(linkDefinitions);
            Assert.Equal("title", definition.Title);
            Assert.True(definition.IsXref);
        }
    }
}
