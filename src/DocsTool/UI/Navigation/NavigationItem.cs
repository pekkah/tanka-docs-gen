using System.Collections.Generic;

namespace Tanka.DocsTool.Navigation
{
    public readonly struct NavigationItem
    {
        public NavigationItem(
            DisplayLink link, 
            IReadOnlyCollection<NavigationItem> children)
        {
            DisplayLink = link;
            Children = children;
        }

        public DisplayLink DisplayLink { get; }

        public IReadOnlyCollection<NavigationItem> Children { get; }

        public override string ToString()
        {
            if (Children.Count > 0)
                return $"{DisplayLink} (children: {Children.Count})";

            return DisplayLink.ToString();
        }
    }
}