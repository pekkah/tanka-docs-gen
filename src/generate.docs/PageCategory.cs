using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace tanka.generate.docs
{
    [DebuggerDisplay("{" + nameof(Category) + "}")]
    internal class PageCategory
    {
        private List<PageInfo> _pages = new List<PageInfo>();

        public PageCategory(string category, string path)
        {
            Category = category ?? path;
            Path = path;
        }

        public List<PageCategory> Categories { get; } = new List<PageCategory>();

        public string Category { get; }
        public string Path { get; }

        public bool HasPages => Pages.Any();

        public IEnumerable<PageInfo> Pages
        {
            get
            {
                var indexPage = _pages.SingleOrDefault(page => page.Path.EndsWith("index.html"));
                return _pages.Except(new[] {indexPage}).ToList();
                return _pages;
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

        public bool HasDisplayName => !string.IsNullOrEmpty(DisplayName);

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

        public PageCategory Add(string name, string path)
        {
            var category = new PageCategory(name, path);
            Categories.Add(category);
            return category;
        }

        public void Add(PageInfo page)
        {
            _pages.Add(page);
        }

        public void Add(IEnumerable<PageInfo> pages)
        {
            foreach (var page in pages)
                Add(page);
        }
    }
}