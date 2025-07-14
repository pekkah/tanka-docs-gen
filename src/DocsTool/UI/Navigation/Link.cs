using System;

namespace Tanka.DocsTool.Navigation
{
    public readonly struct Link : IEquatable<Link>
    {
        public Link(string uri)
        {
            Uri = uri;
            IsXref = false;
            Xref = null;
            IsExternal = true;
        }

        public Link(Xref xref)
        {
            Xref = xref;
            IsXref = true;
            Uri = null;
            IsExternal = false;
        }

        public string? Uri { get; }

        public Xref? Xref { get; }

        public bool IsXref { get; }

        public bool IsExternal { get; }

        public override string ToString()
        {
            if (IsXref)
                return Xref.ToString() ?? string.Empty;

            return Uri ?? string.Empty;
        }

        public bool Equals(Link other)
        {
            return Uri == other.Uri && Nullable.Equals(Xref, other.Xref) && IsXref == other.IsXref && IsExternal == other.IsExternal;
        }

        public override bool Equals(object? obj)
        {
            return obj is Link other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Uri, Xref, IsXref, IsExternal);
        }
    }
}