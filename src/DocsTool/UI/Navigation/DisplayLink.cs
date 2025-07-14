using System;

namespace Tanka.DocsTool.Navigation
{
    public readonly struct DisplayLink : IEquatable<DisplayLink>
    {
        public DisplayLink(string? title, Link link)
        {
            Title = title;
            Link = link;
        }


        public string? Title { get; }

        public Link Link { get; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Title))
                return $"{Link}";

            return $"[{Title}]({Link})";
        }

        public bool Equals(DisplayLink other)
        {
            return Title == other.Title && Link.Equals(other.Link);
        }

        public override bool Equals(object? obj)
        {
            return obj is DisplayLink other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, Link);
        }
    }
}