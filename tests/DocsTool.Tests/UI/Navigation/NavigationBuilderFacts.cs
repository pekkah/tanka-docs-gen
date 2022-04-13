using System;
using System.Collections.Generic;
using System.Text;
using Markdig.Syntax;
using NSubstitute;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Markdown;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;
using Tanka.DocsTool.UI.Navigation;
using Tanka.FileSystem;
using Xunit;

namespace Tanka.DocsTool.Tests.UI.Navigation
{
    public class NavigationBuilderFacts
    {
        private DocsMarkdownService _mdService;
        private DocsSiteRouter _router;
        private NavigationBuilder _sut;

        public NavigationBuilderFacts()
        {
            var source = Substitute.For<IContentSource>();
            source.Version.Returns("TEST");
            source.Path.Returns(new FileSystemPath(""));

            var section = new Section(new ContentItem(
                    source, null ,null),
                new SectionDefinition()
                {
                    Id = "sectionId"
                }, new Dictionary<FileSystemPath, ContentItem>()
                {
                    ["1-example.md"] = new ContentItem(source, null, null),
                    ["1-Subsection/1-first.md"] = new ContentItem(source, null, null),
                    ["1-Subsection/SubSubSection/1-first.md"] = new ContentItem(source, null, null),
                    ["1-Subsection/SubSubSection/1-second.md"] = new ContentItem(source, null, null),
                    ["1-Subsection/1-second.md"] = new ContentItem(source, null, null)
                });

            var site = new Site(
                new SiteDefinition(), 
                new Dictionary<string, Dictionary<string, Section>>()
            {
                ["TEST"] = new Dictionary<string, Section>()
                {
                    [section.Id] = section
                }
            });
            _router = new DocsSiteRouter(site, section);
            _mdService = new DocsMarkdownService(
                new DocsMarkdownRenderingContext(site, section, _router));

            _sut = new NavigationBuilder(_mdService, null);
        }

        [Fact]
        public void Build_Navigation()
        {
            /* Given */
            var md = @"- [Example](xref://1-example.md)
- [Subsection](xref://1-Subsection/1-first.md)
  - [SubSubSection](xref://1-Subsection/SubSubSection/1-first.md)
    - [First](xref://1-Subsection/SubSubSection/1-first.md)
    - [Second](xref://1-Subsection/SubSubSection/1-second.md)
  - [First](xref://1-Subsection/1-first.md)
  - [Second](xref://1-Subsection/1-second.md)
- [Google](https://google.fi)
";

            /* When */
            var menu = _sut.Add(new [] 
            {
                md
            }).Build();

            /* Then */
            Assert.NotNull(menu);
        }
    }
}
