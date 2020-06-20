using System;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;

namespace Tanka.DocsTool.UI
{
    public class DocsSiteRouter
    {
        private readonly Site _site;
        private readonly Section _section;

        public DocsSiteRouter(Site site, Section section)
        {
            _site = site;
            _section = section;
        }

        //todo: should probably move this to Site
        public Xref FullyQualify(Xref xref)
        {
            var targetSection = _site.GetSectionByXref(xref, _section);

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
            var targetSection = _site.GetSectionByXref(xref, _section);

            if (targetSection == null)
                throw new NotImplementedException($"NotFound: {xref}");

            var targetItem = targetSection.GetContentItem(xref.Path);

            if (targetItem == null)
                throw new NotImplementedException($"NotFound: {xref}");

            var path = targetItem.File.Path;

            if (path.GetExtension() == ".md")
                path = path.ChangeExtension(".html");

            return "/"+ targetSection.Version + "/" + targetSection.Path / path;
        }
    }
}