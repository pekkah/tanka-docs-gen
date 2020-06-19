using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Renderers.Normalize.Inlines;

namespace Tanka.DocsTool.Markdown
{
    public class DisplayLinkExtension : IMarkdownExtension
    {
        private readonly DocsMarkdownRenderingContext _context;

        public DisplayLinkExtension(DocsMarkdownRenderingContext context)
        {
            _context = context;
        }

        public DisplayLinkExtension()
        {
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<DisplayLinkInlineParser>())
                // Insert the parser before the link inline parser
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new DisplayLinkInlineParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is NormalizeRenderer normalizeRenderer &&
                !normalizeRenderer.ObjectRenderers.Contains<NormalizeDisplayLinksRenderer>())
                normalizeRenderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(new NormalizeDisplayLinksRenderer());


            if (renderer is HtmlRenderer htmlRenderer &&
                !htmlRenderer.ObjectRenderers.Contains<HtmlDisplayLinksRenderer>())
                htmlRenderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>(
                    new HtmlDisplayLinksRenderer(_context));
        }
    }
}