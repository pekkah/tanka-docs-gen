using Tanka.DocsTool.Pipelines;
using Tanka.DocsTool.UI;

namespace Tanka.DocsTool.Markdown
{
    public class DocsMarkdownRenderingContext
    {
        public DocsMarkdownRenderingContext(
            Site site,
            Section section,
            DocsSiteRouter router,
            BuildContext buildContext)
        {
            Site = site;
            Section = section;
            Router = router;
            BuildContext = buildContext;
        }

        public Site Site { get; }

        public Section Section { get; }

        public DocsSiteRouter Router { get; }

        public BuildContext BuildContext { get; }
    }
}