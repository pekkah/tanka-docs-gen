using System.Collections.Generic;

namespace Tanka.DocsTool.Navigation
{
    public class NavigationBuilder
    {
        private readonly List<DisplayLink> _displayLinks = new List<DisplayLink>();

        public NavigationBuilder AddLinks(string[] displayLinks)
        {
            foreach (var displayLink in displayLinks)
            {
                AddLink(displayLink);
            }

            return this;
        }

        public NavigationBuilder AddLink(string displayLink)
        {
            var link = DisplayLinkParser.Parse(displayLink);
            return AddLink(link);
        }

        public NavigationBuilder AddLink(in DisplayLink displayLink)
        {
            _displayLinks.Add(displayLink);
            return this;
        }

        public IReadOnlyCollection<DisplayLink> Build()
        {
            return _displayLinks;
        }
    }
}