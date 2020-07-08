using System;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;

namespace Tanka.DocsTool.UI
{
    public class DocsSiteRouter
    {
        public Site Site { get; }
        public Section Section { get; }

        public DocsSiteRouter(Site site, Section section)
        {
            Site = site;
            Section = section;
        }

        //todo: should probably move this to Site
        public Xref FullyQualify(Xref xref)
        {
            var targetSection = Site.GetSectionByXref(xref, Section);

            if (targetSection == null)
                throw new NotImplementedException($"NotFound: {xref}");

            var targetItem = targetSection.GetContentItem(xref.Path);

            if (targetItem == null)
                throw new NotImplementedException($"NotFound: {xref}");

            return xref
                .WithSectionId(targetSection.Id)
                .WithVersion(targetSection.Version);
        }

        public string? GenerateRoute(Xref xref)
        {
            var targetSection = Site.GetSectionByXref(xref, Section);

            if (targetSection == null)
                throw new NotImplementedException($"Section NotFound: {xref}");

            var targetItem = targetSection.GetContentItem(xref.Path);

            if (targetItem == null)
                throw new NotImplementedException($"ContentItem NotFound: {xref} from section {targetSection}");

            Path path = xref.Path;

            if (path.GetExtension() == ".md")
                path = path.ChangeExtension(".html");

            return "/"+ targetSection.Version + "/" + targetSection.Path / path;
        }
    }
}