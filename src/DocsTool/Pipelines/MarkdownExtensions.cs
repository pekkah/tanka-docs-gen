using System.IO;
using System.Threading.Tasks;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Renderers.Normalize.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Pipelines
{
    public static class MarkdownExtensions
    {
        public static async Task<MarkdownDocument> ParseMarkdown(this ContentItem item)
        {
            await using var stream = await item.File.OpenRead();
            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync();

            return ParseMarkdown(text);
        }

        public static MarkdownDocument ParseMarkdown(this string text)
        {
            var builder = new MarkdownPipelineBuilder();
            builder.UseYamlFrontMatter();
            builder.Use<DisplayLinkExtension>();

            var pipeline = builder.Build();
            var document = MarkdownParser.Parse(text, pipeline);
            return document;
        }
    }

    public class DisplayLinkExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<DisplayLinkInlineParser>())
                // Insert the parser before the link inline parser
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new DisplayLinkInlineParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var normalizeRenderer = renderer as NormalizeRenderer;
            if (normalizeRenderer != null &&
                !normalizeRenderer.ObjectRenderers.Contains<NormalizeDisplayLinksRenderer>())
                normalizeRenderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(new NormalizeDisplayLinksRenderer());
        }
    }

    public class DisplayLinkInlineParser : InlineParser
    {
        public DisplayLinkInlineParser()
        {
            OpeningCharacters = "[".ToCharArray();
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var startPosition = processor.GetSourcePosition(slice.Start, out var startLine, out var startCiColumn);

            var start = slice.Start;
            var end = start;

            // skip opening '['
            var current = slice.NextChar();

            while (current.IsAlphaNumeric())
            {
                end = slice.Start;
                current = slice.NextChar();
            }

            // label should end at ']'
            if (current != ']')
                return false;

            // skip ']'
            current = slice.NextChar();

            // uri part should start with '('
            if (current != '(')
                return false;

            // skip '('
            current = slice.NextChar();

            while (current != ')' && current != '\0')
            {
                end = slice.Start;
                current = slice.NextChar();
            }

            // uri part should end with ')'
            if (current != ')')
                return false;

            end = slice.Start;
            slice.NextChar();

            var endPosition = processor.GetSourcePosition(slice.Start - 1);
            var linkText = new StringSlice(slice.Text, start, end).ToString();

            processor.Inline = new DisplayLinkInline
            {
                Span = new SourceSpan(startPosition, endPosition),
                Line = startLine,
                Column = startCiColumn,
                IsClosed = true,
                DisplayLink = DisplayLinkParser.Parse(linkText)
            };

            return true;
        }
    }

    public class NormalizeDisplayLinksRenderer : NormalizeObjectRenderer<DisplayLinkInline>
    {
        protected override void Write(NormalizeRenderer renderer, DisplayLinkInline obj)
        {
            renderer.Write(obj.DisplayLink.ToString());
        }
    }

    public class DisplayLinkInline : Inline
    {
        public DisplayLinkInline()
        {
            IsClosed = true;
        }

        public DisplayLink DisplayLink { get; set; }
    }
}