using System.Collections.Generic;
using System.IO;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Definitions
{
    public class SiteDefinition
    {
        public string Title { get; set; } = "Tanka Docs";

        public Xref IndexPage { get; set; } = LinkParser.Parse("xref://root:index.md").Xref!.Value;

        public string OutputPath { get; set; } = "output";

        public string BuildPath { get; set; } = Path.GetTempPath();

        public IReadOnlyDictionary<string, BranchDefinition> Branches { get; set; } = new Dictionary<string, BranchDefinition>();

        public string[]? Tags { get; set; }

        public string BasePath { get; set; } = "";
    }

    public class BranchDefinition
    {
        public string? Title { get; set; }

        public string[] InputPath { get; set; } = { "docs" };
    }
}