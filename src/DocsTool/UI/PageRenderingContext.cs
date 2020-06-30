using System.Collections.Generic;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool.UI
{
    public class PageRenderingContext
    {
        public PageRenderingContext(Site site, Section section, IReadOnlyCollection<NavigationItem> menu, PageFrontmatter page, string pageHtml)
        {
            Site = site;
            Section = section;
            Menu = menu;
            Page = page;
            PageHtml = pageHtml;
        }


        public Site Site { get; }

        public Section Section { get; }

        public IReadOnlyCollection<NavigationItem> Menu { get; }

        public PageFrontmatter Page { get; }

        public string PageHtml { get; }
    }
}