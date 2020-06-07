using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;
using Xunit;

namespace Tanka.DocsTool.Tests.Pipelines
{
    public class RouterFacts
    {
        private Router _sut;

        public RouterFacts()
        {
            _sut = new Router();
        }

        [Fact]
        public void Shoud_use_section_for_version_and_id_when_xref_has_only_path()
        {
            /* Given */
            Xref xref = new Xref(null, null, "page.md");
            SectionModel section = new SectionModel(
                "master", 
                new SectionDefinition()
                {
                    Id = "root"
                },
                new NavigationItem[0],
                new ContentItem[0]);
            
            /* When */
            var htmlOutputPath = _sut.GetOutputPath(section, xref, "html");

            /* Then */
            Assert.Equal("master/root/page.html", htmlOutputPath);
        }

        [Fact]
        public void Shoud_use_xref_version_and_section_when_present()
        {
            /* Given */
            Xref xref = new Xref("2.0.0", "tutorials", "index.md");
            SectionModel section = new SectionModel(
                "master",
                new SectionDefinition()
                {
                    Id = "root"
                },
                new NavigationItem[0],
                new ContentItem[0]);

            /* When */
            var htmlOutputPath = _sut.GetOutputPath(section, xref, "html");

            /* Then */
            Assert.Equal("2.0.0/tutorials/index.html", htmlOutputPath);
        }
    }
}