using System;
using System.Collections.Generic;

namespace Tanka.DocsTool.Navigation
{
    public readonly struct NavigationItem : IEquatable<NavigationItem>
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

        public string? Title => DisplayLink.Title;

        public override string ToString()
        {
            if (Children.Count > 0)
                return $"{DisplayLink} (children: {Children.Count})";

            return DisplayLink.ToString();
        }

        public bool Equals(NavigationItem other)
        {
            if (Children == null)
                return DisplayLink.Equals(other.DisplayLink);

            return DisplayLink.Equals(other.DisplayLink) && Children.Equals(other.Children);
        }

        public override bool Equals(object? obj)
        {
            return obj is NavigationItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DisplayLink, Children);
        }
    }
}