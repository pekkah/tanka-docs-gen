using System.Collections.Generic;
using System.IO;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Definitions
{
    public class SiteDefinition
    {
        public string Title { get; set; } = "Tanka Docs";

        public Link IndexSection { get; set; } = LinkParser.Parse("xref://root:index.md");

        public string? InputPath { get; set; }

        public string? OutputPath { get; set; }

        public string? BuildPath { get; set; } = Path.GetTempPath();

        public IReadOnlyDictionary<string, string> Branches { get; set; } = new Dictionary<string, string>();

        public string[]? Tags { get; set; }
    }
}