using System.Collections.Generic;
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

        public Dictionary<string, Dictionary<string, object>> Extensions { get; set; } = new Dictionary<string, Dictionary<string, object>>();

        public string[]? Includes { get; set; }
    }
}
