using Markdig.Renderers.Normalize;

namespace Tanka.DocsTool.Markdown
{
    public class NormalizeDisplayLinksRenderer : NormalizeObjectRenderer<DisplayLinkInline>
    {
        protected override void Write(NormalizeRenderer renderer, DisplayLinkInline obj)
        {
            renderer.Write(obj.DisplayLink.ToString());
        }
    }
}