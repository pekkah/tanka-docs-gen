using Markdig.Syntax.Inlines;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Markdown
{
    public class DisplayLinkInline : Inline
    {
        public DisplayLinkInline()
        {
            IsClosed = true;
        }

        public DisplayLink DisplayLink { get; set; }
    }
}