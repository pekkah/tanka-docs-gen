using System;
using System.Collections.Generic;
using System.IO;

namespace Tanka.DocsTool.Definitions
{
    public class SiteDefinition
    {
        public string Title { get; set; } = "Tanka Docs";

        public string IndexSection { get; set; } = "xref:ROOT:index.md";

        public string? InputPath { get; set; }

        public string? OutputPath { get; set; }

        public string? BuildPath { get; set; } = Path.GetTempPath();

        public IReadOnlyDictionary<string, string> Branches { get; set; }

        public string[]? Tags { get; set; }
    }
}