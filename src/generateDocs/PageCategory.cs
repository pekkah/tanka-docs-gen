using System;
using System.Collections.Generic;

namespace tanka.generate.docs
{
    internal class PageCategory
    {
        public PageCategory(string category, IEnumerable<PageInfo> pages)
        {
            Category = string.IsNullOrEmpty(category) ? "Home" : category;
            Pages = pages ?? throw new ArgumentNullException(nameof(pages));
        }

        public string Category { get; }

        public IEnumerable<PageInfo> Pages { get; }

        public string DisplayName
        {
            get
            {
                var displayName = Category.Replace('-', ' ');

                if (string.IsNullOrEmpty(displayName))
                    return string.Empty;

                displayName = displayName.Substring(0, 1).ToUpperInvariant() + displayName.Substring(1);
                return displayName;
            }
        }
    }
}