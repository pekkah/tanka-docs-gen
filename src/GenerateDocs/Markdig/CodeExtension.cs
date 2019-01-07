using System;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Fugu.GenerateDocs.Markdig
{
    public class CodeExtension : IMarkdownExtension
    {
        private readonly PipelineContext _context;

        public CodeExtension(PipelineContext context)
        {
            _context = context;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new IncludeCodeParser(_context));
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer)
            {
                renderer.ObjectRenderers.AddIfNotAlready(new IncludeCodeRenderer(_context));
            }
        }
    }

    public class IncludeCodeRenderer: HtmlObjectRenderer<CodeInclude>
    {
        private readonly PipelineContext _context;

        public IncludeCodeRenderer(PipelineContext context)
        {
            _context = context;
        }

        protected override void Write(HtmlRenderer renderer, CodeInclude codeInclude)
        {
            var code = _context.Solution.GetSourceText(codeInclude.DisplayName);
            renderer.Write("<pre><code class=\"language-csharp\">");
            renderer.WriteEscape(code.Text);
            renderer.Write("</code></pre>");
        }
    }

    /// <summary>
    ///     [{DisplayName}]
    /// </summary>
    public class IncludeCodeParser : InlineParser
    {
        private PipelineContext _context;

        public IncludeCodeParser(PipelineContext context)
        {
            _context = context;
            OpeningCharacters = new[] {'['};
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var nextChar = slice.PeekChar();

            if (nextChar != '{')
            {
                return false;
            }

            var start = slice.Start;
            slice.NextChar();
            nextChar = slice.NextChar();

            var contentBuilder = processor.StringBuilders.Get();
            while (nextChar != '}')
            {
                contentBuilder.Append(nextChar);
                nextChar = slice.NextChar();
            }
  
            if (slice.PeekChar() != ']')
            {
                processor.StringBuilders.Release(contentBuilder);
                return false;
            }

            slice.NextChar();
            slice.NextChar();

            processor.Inline = new CodeInclude()
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