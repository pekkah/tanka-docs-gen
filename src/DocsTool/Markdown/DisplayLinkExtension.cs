using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Renderers.Normalize.Inlines;

namespace Tanka.DocsTool.Markdown
{
    public class DisplayLinkExtension : IMarkdownExtension
    {
        private readonly DocsMarkdownRenderingContext? _context; // CS8618

        public DisplayLinkExtension(DocsMarkdownRenderingContext context)
        {
            _context = context;
        }

        public DisplayLinkExtension()
        {
            // _context remains null here. If this constructor is used,
            // _context must be handled as potentially null by consuming code
            // or ensured to be set by other means if methods relying on it are called.
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<DisplayLinkInlineParser>())
                // Insert the parser before the link inline parser
                // Assuming DisplayLinkInlineParser constructor can handle a null context or
                // this path implies _context should have been initialized if this setup is reached.
                // Using null-forgiving operator as a temporary measure if it cannot be null.
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new DisplayLinkInlineParser(_context!));
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is NormalizeRenderer normalizeRenderer &&
                !normalizeRenderer.ObjectRenderers.Contains<NormalizeDisplayLinksRenderer>())
                normalizeRenderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(new NormalizeDisplayLinksRenderer());


            if (renderer is HtmlRenderer htmlRenderer &&
                !htmlRenderer.ObjectRenderers.Contains<HtmlDisplayLinksRenderer>())
                // Assuming HtmlDisplayLinksRenderer constructor can handle a null context or
                // this path implies _context should have been initialized if this setup is reached.
                // Using null-forgiving operator as a temporary measure if it cannot be null.
                htmlRenderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>(
                    new HtmlDisplayLinksRenderer(_context!));
        }
    }
}