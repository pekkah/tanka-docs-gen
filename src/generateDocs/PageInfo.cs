using System;
using System.Globalization;

namespace tanka.generate.docs
{
    internal class PageInfo : IEquatable<PageInfo>
    {
        public PageInfo(string path, bool isActive = false)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            IsActive = isActive;
        }

        public string Path { get; }

        public bool IsActive { get; }

        public string Href
        {
            get
            {
                var href = Path.Replace('\\', '/');
                return $"{href}";
            }
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Path))
                    return string.Empty;

                var displayName = Path;

                displayName = System.IO.Path.GetFileNameWithoutExtension(displayName);

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

        public bool Equals(PageInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DisplayName, other.DisplayName);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PageInfo);
        }

        public override int GetHashCode()
        {
            return Path != null ? Path.GetHashCode() : 0;
        }

        public static bool operator ==(PageInfo left, PageInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PageInfo left, PageInfo right)
        {
            return !Equals(left, right);
        }
    }
}