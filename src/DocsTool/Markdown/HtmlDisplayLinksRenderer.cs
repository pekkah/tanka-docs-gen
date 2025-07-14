using Markdig.Renderers;
using Markdig.Renderers.Html;
using Tanka.DocsTool.UI;

namespace Tanka.DocsTool.Markdown
{
    public class HtmlDisplayLinksRenderer : HtmlObjectRenderer<DisplayLinkInline>
    {
        private readonly DocsSiteRouter _router;
        private readonly DocsMarkdownRenderingContext? _context;

        public HtmlDisplayLinksRenderer(DocsMarkdownRenderingContext context)
        {
            _router = context.Router;
            _context = context;
        }

        protected override void Write(HtmlRenderer renderer, DisplayLinkInline obj)
        {
            string? url;

            if (obj.DisplayLink.Link.IsXref)
            {
                var xref = obj.DisplayLink.Link.Xref!.Value;
                if (_context?.BuildContext != null)
                {
                    url = _router.GenerateRoute(xref, _context.BuildContext);
                }
                else
                {
                    // Fallback to old behavior if no BuildContext available
                    url = _router.GenerateRoute(xref);
                }
            }
            else
            {
                url = obj.DisplayLink.Link.Uri;
            }

            renderer.Write("<a href=\"");
            renderer.WriteEscapeUrl(url);
            renderer.Write("\"");

            // Add CSS class for broken xrefs in relaxed mode
            if (obj.DisplayLink.Link.IsXref && url?.StartsWith("#broken-xref-") == true)
            {
                renderer.Write(" class=\"broken-xref\"");
                renderer.Write($" data-original-xref=\"{obj.DisplayLink.Link.Xref}\"");
            }

            renderer.Write(">");
            renderer.Write(obj.DisplayLink.Title ?? "[Title is missing]");
            renderer.Write("</a>");
        }
    }
}