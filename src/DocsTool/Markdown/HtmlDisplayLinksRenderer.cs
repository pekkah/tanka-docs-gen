using Markdig.Renderers;
using Markdig.Renderers.Html;
using Tanka.DocsTool.UI;

namespace Tanka.DocsTool.Markdown
{
    public class HtmlDisplayLinksRenderer : HtmlObjectRenderer<DisplayLinkInline>
    {
        private readonly DocsSiteRouter _router;

        public HtmlDisplayLinksRenderer(DocsMarkdownRenderingContext context)
        {
            _router = context.Router;
        }

        protected override void Write(HtmlRenderer renderer, DisplayLinkInline obj)
        {
            var url = obj.DisplayLink.Link.IsXref
                ? _router.GenerateRoute(obj.DisplayLink.Link.Xref!.Value)
                : obj.DisplayLink.Link.Uri;

            renderer.Write("<a href=\"");
            renderer.WriteEscapeUrl(url);
            renderer.Write("\"");
            renderer.Write(">");
            renderer.Write(obj.DisplayLink.Title ?? "[Title is missing]");
            renderer.Write("</a>");
        }
    }
}