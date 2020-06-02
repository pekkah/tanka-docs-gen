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

        public override string ToString()
        {
            if (Children.Count > 0)
                return $"{Link} (children: {Children.Count})";

            return Link.ToString();
        }
    }
}