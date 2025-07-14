using System;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.UI;

namespace Tanka.DocsTool.Markdown
{
    public class DisplayLinkInlineParser : InlineParser
    {
        private readonly DocsSiteRouter _router;
        private readonly DocsMarkdownRenderingContext? _context;

        public DisplayLinkInlineParser(DocsMarkdownRenderingContext context)
        {
            _router = context.Router;
            _context = context;
            OpeningCharacters = "[".ToCharArray();
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var startPosition = processor.GetSourcePosition(slice.Start, out var startLine, out var startCiColumn);

            var start = slice.Start;
            var end = start;

            // skip opening '['
            var current = slice.NextChar();

            while (current != ']' && current != '\0')
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

            try
            {
                var link = DisplayLinkParser.Parse(linkText);

                if (link.Link.IsXref)
                {
                    var xref = link.Link.Xref!.Value;

                    if (_context?.BuildContext != null)
                    {
                        var fullyQualifiedXref = _router.FullyQualify(xref, _context.BuildContext);

                        if (fullyQualifiedXref != null)
                        {
                            link = new DisplayLink(
                                link.Title,
                                new Link(fullyQualifiedXref.Value));
                        }
                        // If xref validation failed, keep the original link for placeholder generation in renderer
                    }
                    else
                    {
                        // Fallback to old behavior if no BuildContext available
                        link = new DisplayLink(
                            link.Title,
                            new Link(_router.FullyQualify(xref)));
                    }
                }

                processor.Inline = new DisplayLinkInline
                {
                    Span = new SourceSpan(startPosition, endPosition),
                    Line = startLine,
                    Column = startCiColumn,
                    IsClosed = true,
                    DisplayLink = link
                };
            }
            catch (Exception x)
            {
                throw new InvalidOperationException(
                    $"Invalid link '{linkText}'", x);
            }

            return true;
        }
    }
}