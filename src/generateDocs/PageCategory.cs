using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace tanka.generate.docs
{
    internal class PageCategory
    {
        private readonly List<PageInfo> _pages;

        public PageCategory(string category, IEnumerable<PageInfo> pages)
        {
            Category = string.IsNullOrEmpty(category) ? "Home" : category;
            _pages = pages?.ToList() ?? throw new ArgumentNullException(nameof(pages));
        }

        public string Category { get; }

        public IEnumerable<PageInfo> Pages
        {
            get
            {
                if (!_pages.Any())
                    return Enumerable.Empty<PageInfo>();

                var indexPage = _pages.SingleOrDefault(page => page.Path.EndsWith("index.html"));

                if (_pages.Count == 1)
                {
                    if (indexPage != null)
                    {
                        return Enumerable.Empty<PageInfo>();
                    }
                }

                var pages = new List<PageInfo>(_pages);

                if (indexPage != null)
                {
                    pages.Remove(indexPage);
                }

                return pages;
            }
        }

        public string Href
        {
            get
            {
                if (!_pages.Any())
                    return string.Empty;

                var indexPage = _pages.SingleOrDefault(page => page.Path.EndsWith("index.html"));

                if (indexPage != null)
                    return indexPage.Href;

                return _pages.First().Href;
            }
        }

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

                    if (hasSection) displayName = displayName.Replace(maybeSection, string.Empty);
                }

                displayName = displayName.Replace('-', ' ');
                displayName = displayName.Substring(0, 1).ToUpperInvariant() + displayName.Substring(1);
                return displayName;
            }
        }
    }
}