using System.Text;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Tanka.DocsTool.Extensions.Roslyn
{
    public class CodeExtension : IMarkdownExtension
    {
        private readonly SolutionContext _context;

        public CodeExtension(SolutionContext context)
        {
            _context = context;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (_context != null)
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new IncludeCodeParser(_context));
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (_context != null)
                if (renderer is HtmlRenderer)
                    renderer.ObjectRenderers.AddIfNotAlready(new IncludeCodeRenderer(_context));
        }
    }

    public class IncludeCodeRenderer : HtmlObjectRenderer<CodeInclude>
    {
        private readonly SolutionContext _context;

        public IncludeCodeRenderer(SolutionContext context)
        {
            _context = context;
        }

        protected override void Write(HtmlRenderer renderer, CodeInclude codeInclude)
        {
            var code = _context.GetSourceText(codeInclude.DisplayName);

            if (code.NotFound)
            {
                renderer.Write("<pre><code class=\"language-csharp\">");
                renderer.Write($"NOT_FOUND: {codeInclude.DisplayName}");
                renderer.Write("</code></pre>");
            }
            else
            {
                renderer.Write("<pre><code class=\"language-csharp\">");
                renderer.WriteEscape(code.Text, 0, code.Text.Length);
                renderer.Write("</code></pre>");
            }
        }
    }

    /// <summary>
    ///     [{DisplayName}]
    /// </summary>
    public class IncludeCodeParser : InlineParser
    {
        private SolutionContext _context;

        public IncludeCodeParser(SolutionContext context)
        {
            _context = context;
            OpeningCharacters = new[] {'['};
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var nextChar = slice.PeekChar();

            if (nextChar != '{') return false;

            var start = slice.Start;
            slice.NextChar();
            nextChar = slice.NextChar();

            var contentBuilder = new StringBuilder();
            while (nextChar != '}')
            {
                contentBuilder.Append(nextChar);
                nextChar = slice.NextChar();
            }

            if (slice.PeekChar() != ']')
            {
                return false;
            }

            slice.NextChar();
            slice.NextChar();

            processor.Inline = new CodeInclude
            {
                DisplayName = contentBuilder.ToString(),
                Span = new SourceSpan(processor.GetSourcePosition(
                        start, out var line, out var column),
                    processor.GetSourcePosition(slice.End)),
                Line = line,
                Column = column
            };

            return true;
        }
    }

    public class CodeInclude : Inline
    {
        public string DisplayName { get; set; }
    }
}