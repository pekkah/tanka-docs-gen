using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Navigation;

namespace Tanka.DocsTool.Pipelines
{
    public class Site
    {
        private readonly Dictionary<string, Dictionary<string, Section>> _sectionsByVersion;

        public SiteDefinition Definition { get; }

        public Site(SiteDefinition definition, Dictionary<string, Dictionary<string, Section>> sectionsByVersion)
        {
            _sectionsByVersion = sectionsByVersion;
            Definition = definition;
        }

        public string Title => Definition.Title;

        public string BasePath => Definition.BasePath;

        public IReadOnlyCollection<string> Versions => _sectionsByVersion.Keys;

        public IEnumerable<Section> GetSectionsByVersion(string version)
        {
            if (_sectionsByVersion.TryGetValue(version, out var sectionsById))
                return sectionsById.Values;

            return Enumerable.Empty<Section>();
        }

        public Section? GetSectionByXref(Xref xref, Section context)
        {
            if (xref.Version == null)
                xref = xref.WithVersion(context.Version);

            if (xref.SectionId == null)
                xref = xref.WithSectionId(context.Id);

            return GetSectionByXref(xref);
        }

        private Section? GetSectionByXref(in Xref xref)
        {
            if (xref.SectionId == null)
                throw new ArgumentNullException(nameof(xref.SectionId));

            if (xref.Version == null)
                throw new ArgumentNullException(nameof(xref.Version));

            if (_sectionsByVersion.TryGetValue(xref.Version, out var sectionsById))
            {
                if (sectionsById.TryGetValue(xref.SectionId, out var section))
                    return section;
            }

            return null;
        }
    }
}