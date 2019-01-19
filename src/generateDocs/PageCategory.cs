using System;
using System.Collections.Generic;
using System.Globalization;

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
                if (string.IsNullOrEmpty(Category))
                    return string.Empty;

                var displayName = Category;

                if (displayName.Contains("-"))
                {
                    var maybeSection = displayName.Substring(0, displayName.IndexOf('-') + 1);
                    var hasSection = float.TryParse(maybeSection, NumberStyles.Any, NumberFormatInfo.InvariantInfo,
                        out _);

                    if (hasSection)
                    {
                        displayName = displayName.Replace(maybeSection, string.Empty);
                    }
                }

                displayName = displayName.Replace('-', ' ');
                displayName = displayName.Substring(0, 1).ToUpperInvariant() + displayName.Substring(1);
                return displayName;
            }
        }
    }
}