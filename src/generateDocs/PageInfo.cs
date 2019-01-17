using System;

namespace tanka.generate.docs
{
    internal class PageInfo
    {
        public PageInfo(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path { get; }

        public string Href
        {
            get
            {
                var href = Path.Replace('\\', '/');
                return href;
            }
        }

        public string DisplayName
        {
            get
            {
                var displayName = Path.Replace('-', ' ');

                if (string.IsNullOrEmpty(displayName))
                    return string.Empty;

                displayName = System.IO.Path.GetFileNameWithoutExtension(displayName);
                displayName = displayName.Substring(0, 1).ToUpperInvariant() + displayName.Substring(1);
                return displayName;
            }
        }
    }
}