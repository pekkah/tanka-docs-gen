﻿namespace Tanka.DocsTool.Extensions.Roslyn
{
    public class SourceCode
    {
        public string Text { get; set; }

        public string FileName { get; set; }

        public string Span { get; set; }

        public bool NotFound { get; set; }
    }
}