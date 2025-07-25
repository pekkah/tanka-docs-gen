﻿using Tanka.DocsTool.Catalogs;
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
            // Check for HEAD version which is not allowed in xref links
            // Exception: HEAD is allowed if it's actually configured as a version in the site
            if (xref.Version?.Equals("HEAD", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Check if HEAD is a valid version in the current site configuration
                if (!Site.Versions.Contains("HEAD"))
                {
                    var message = $"Invalid xref reference: {xref} - HEAD version is not allowed. Use a specific version or omit the version to use the current context.";
                    buildContext.Add(new Error(message, contentItem));
                    return null;
                }
            }

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
            // Check for HEAD version which is not allowed in xref links
            // Exception: HEAD is allowed if it's actually configured as a version in the site
            if (xref.Version?.Equals("HEAD", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Check if HEAD is a valid version in the current site configuration
                if (!Site.Versions.Contains("HEAD"))
                {
                    var message = $"Invalid xref reference: {xref} - HEAD version is not allowed. Use a specific version or omit the version to use the current context.";
                    buildContext.Add(new Error(message, contentItem));
                    return $"#broken-xref-{xref.ToString().GetHashCode()}";
                }
            }

            var targetSection = Site.GetSectionByXref(xref, Section);
            var targetItem = targetSection?.GetContentItem(xref.Path);

            if (targetSection == null || targetItem == null)
            {
                var message = targetSection == null
                    ? $"Broken xref reference: {xref} - section not found"
                    : $"Broken xref reference: {xref} - content item not found";
                
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

        /// <summary>
        /// Generates route for asset files (delegates to GenerateRoute for consistency)
        /// Provides semantic separation for future asset-specific routing changes
        /// </summary>
        public string? GenerateAssetRoute(Xref xref) => GenerateRoute(xref);

        /// <summary>
        /// Generates route for asset files (delegates to GenerateRoute for consistency)
        /// Provides semantic separation for future asset-specific routing changes
        /// </summary>
        public string? GenerateAssetRoute(Xref xref, BuildContext buildContext, ContentItem? contentItem = null) 
            => GenerateRoute(xref, buildContext, contentItem);
    }
}