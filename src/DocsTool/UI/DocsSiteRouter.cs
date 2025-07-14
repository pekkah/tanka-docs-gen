using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;

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

        public Xref FullyQualify(Xref xref)
        {
            var targetSection = Site.GetSectionByXref(xref, Section);

            if (targetSection == null)
                throw new InvalidOperationException($"NotFound: {xref}");

            var targetItem = targetSection.GetContentItem(xref.Path);

            if (targetItem == null)
                throw new InvalidOperationException($"NotFound: {xref}");

            return xref
                .WithSectionId(targetSection.Id)
                .WithVersion(targetSection.Version);
        }

        public Xref? FullyQualify(Xref xref, BuildContext buildContext, ContentItem? contentItem = null)
        {
            var targetSection = Site.GetSectionByXref(xref, Section);

            if (targetSection == null)
            {
                var message = $"Broken xref reference: {xref} - section not found";
                if (buildContext.LinkValidation == LinkValidation.Strict)
                    buildContext.Add(new Error(message, contentItem));
                else
                    buildContext.Add(new Error(message, contentItem), isWarning: true);
                return null;
            }

            var targetItem = targetSection.GetContentItem(xref.Path);

            if (targetItem == null)
            {
                var message = $"Broken xref reference: {xref} - content item not found";
                if (buildContext.LinkValidation == LinkValidation.Strict)
                    buildContext.Add(new Error(message, contentItem));
                else
                    buildContext.Add(new Error(message, contentItem), isWarning: true);
                return null;
            }

            return xref
                .WithSectionId(targetSection.Id)
                .WithVersion(targetSection.Version);
        }

        public string? GenerateRoute(Xref xref)
        {
            var targetSection = Site.GetSectionByXref(xref, Section);

            if (targetSection == null)
                throw new InvalidOperationException($"Section NotFound: {xref}");

            FileSystemPath path = xref.Path;

            if (path.GetExtension() == ".md")
                path = path.ChangeExtension(".html");

            return targetSection.Version + "/" + targetSection.Path / path;
        }

        public string? GenerateRoute(Xref xref, BuildContext buildContext, ContentItem? contentItem = null)
        {
            var targetSection = Site.GetSectionByXref(xref, Section);

            if (targetSection == null)
            {
                var message = $"Broken xref reference: {xref} - section not found";
                if (buildContext.LinkValidation == LinkValidation.Strict)
                    buildContext.Add(new Error(message, contentItem));
                else
                    buildContext.Add(new Error(message, contentItem), isWarning: true);

                // Generate placeholder link for relaxed mode
                var sanitizedXref = xref.ToString().Replace(":", "-").Replace("/", "-").Replace("@", "-");
                return $"#broken-xref-{sanitizedXref}";
            }

            FileSystemPath path = xref.Path;

            if (path.GetExtension() == ".md")
                path = path.ChangeExtension(".html");

            return targetSection.Version + "/" + targetSection.Path / path;
        }
    }
}