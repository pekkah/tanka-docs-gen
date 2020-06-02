namespace Tanka.DocsTool.Navigation
{
    public readonly struct Link
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
    }
}