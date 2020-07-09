using System.Collections.Generic;
using System.IO;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Definitions
{
    public class SiteDefinition
    {
        public string Title { get; set; } = "Tanka Docs";

        public Link IndexPage { get; set; } = LinkParser.Parse("xref://root@HEAD:index.md");

        public string OutputPath { get; set; } = "output";

        public string BuildPath { get; set; } = Path.GetTempPath();

        public Dictionary<string, BranchDefinition> Branches { get; set; } = new Dictionary<string, BranchDefinition>();

        public Dictionary<string, BranchDefinition> Tags { get; set; } = new Dictionary<string, BranchDefinition>();

        public string BasePath { get; set; } = "";

        public Dictionary<string, Dictionary<string, object>> Extensions { get; set; } = new Dictionary<string, Dictionary<string, object>>();
    }

    public class BranchDefinition
    {
        public string? Title { get; set; }

        public string[] InputPath { get; set; } = { "docs" };
    }
}