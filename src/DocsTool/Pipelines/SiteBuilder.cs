using System.Collections.Generic;
using Tanka.DocsTool.Definitions;

namespace Tanka.DocsTool.Pipelines
{
    public class SiteBuilder
    {
        private readonly SiteDefinition _definition;

        private readonly Dictionary<string, Dictionary<string, Section>> _sectionsByVersion 
        = new Dictionary<string, Dictionary<string, Section>>();

        public SiteBuilder(SiteDefinition definition)
        {
            _definition = definition;
        }

        public SiteBuilder Add(IReadOnlyCollection<Section> sections)
        {
            foreach (var section in sections)
            {
                Add(section);
            }

            return this;
        }

        public SiteBuilder Add(Section section)
        {
            var version = section.Version;
            var id = section.Id;

            if (!_sectionsByVersion.TryGetValue(version, out var sectionsById))
            {
                sectionsById = new Dictionary<string, Section>();
                _sectionsByVersion.Add(version, sectionsById);
            }

            sectionsById.Add(id, section);
            return this;
        }

        public Site Build()
        {
            return new Site(_definition, _sectionsByVersion);
        }
    }
}