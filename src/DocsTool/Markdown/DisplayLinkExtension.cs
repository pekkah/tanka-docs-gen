using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Renderers.Normalize;
using Markdig.Renderers.Normalize.Inlines;
using Markdig.Syntax.Inlines;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.UI;

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
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new DisplayLinkInlineParser(_context));
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is NormalizeRenderer normalizeRenderer &&
                !normalizeRenderer.ObjectRenderers.Contains<NormalizeDisplayLinksRenderer>())
                normalizeRenderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Normalize.Inlines.LinkInlineRenderer>(new NormalizeDisplayLinksRenderer());


            if (renderer is HtmlRenderer htmlRenderer &&
                !htmlRenderer.ObjectRenderers.Contains<HtmlDisplayLinksRenderer>())
            {
                htmlRenderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>(
                    new HtmlDisplayLinksRenderer(_context));
                
                // Add support for xref:// images
                var linkRenderer = htmlRenderer.ObjectRenderers.FindExact<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>();
                if (linkRenderer != null)
                {
                    linkRenderer.TryWriters.Add(TryXrefImageRenderer);
                }
            }
        }

        private bool TryXrefImageRenderer(HtmlRenderer renderer, LinkInline linkInline)
        {
            // Only handle images with xref:// URLs
            if (!linkInline.IsImage || linkInline.Url == null || !linkInline.Url.StartsWith("xref://"))
                return false;

            // Parse the xref URL
            var xrefLink = LinkParser.Parse(linkInline.Url);
            if (!xrefLink.IsXref)
                return false;

            // Use GenerateAssetRoute for semantic clarity and track cross-section assets
            string? resolvedUrl;
            if (_context?.BuildContext != null)
            {
                resolvedUrl = _context.Router.GenerateAssetRoute(xrefLink.Xref!.Value, _context.BuildContext);
                
                // Track cross-section assets for copying (thread-safe)
                var targetSection = _context.Router.Site.GetSectionByXref(xrefLink.Xref!.Value, _context.Section);
                var targetItem = targetSection?.GetContentItem(xrefLink.Xref!.Value.Path);
                
                if (targetSection != null && targetItem != null && 
                    AssetConfiguration.IsAsset(xrefLink.Xref!.Value.Path, targetSection.Definition))
                {
                    _context.BuildContext.TrackXrefAsset(xrefLink.Xref!.Value, _context.Section, targetSection, targetItem);
                }
            }
            else
            {
                resolvedUrl = _context.Router.GenerateAssetRoute(xrefLink.Xref!.Value);
            }

            // Render the image
            renderer.Write("<img src=\"");
            renderer.WriteEscapeUrl(resolvedUrl);
            renderer.Write("\"");

            // Add CSS class for broken xrefs in relaxed mode
            if (resolvedUrl?.StartsWith("#broken-xref-") == true)
            {
                renderer.Write(" class=\"broken-xref-image\"");
                renderer.Write($" data-original-xref=\"{xrefLink.Xref}\"");
            }
            else
            {
                renderer.Write(" class=\"img-fluid\"");
            }

            // Add alt text from the markdown
            if (linkInline.FirstChild != null)
            {
                renderer.Write(" alt=\"");
                var wasEnableHtmlForInline = renderer.EnableHtmlForInline;
                renderer.EnableHtmlForInline = false;
                renderer.WriteChildren(linkInline);
                renderer.EnableHtmlForInline = wasEnableHtmlForInline;
                renderer.Write("\"");
            }

            renderer.Write(" />");

            return true; // Indicates we handled the rendering
        }
    }
}