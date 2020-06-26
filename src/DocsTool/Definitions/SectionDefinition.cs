using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Definitions
{
    public class SectionDefinition
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public Link IndexPage { get; set; } = LinkParser.Parse("xref://index.md");

        public Link[] Nav { get; set; } = new Link[0];

        public string Type { get; set; } = "doc";
    }
}
