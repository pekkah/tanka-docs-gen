using System.Collections.Generic;

namespace Tanka.DocsTool.Navigation
{
    public readonly struct NavigationItem
    {
        public NavigationItem(
            DisplayLink link, 
            IReadOnlyCollection<NavigationItem> children)
        {
            Link = link;
            Children = children;
        }

        public DisplayLink Link { get; }

        public IReadOnlyCollection<NavigationItem> Children { get; }
    }
}