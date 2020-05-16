namespace Tanka.DocsTool.Navigation
{
    public readonly struct Link
    {
        public Link(string? title, string uri)
        {
            Title = title;
            Uri = uri;
            IsXref = false;
            Xref = null;
            IsExternal = true;
        }

        public Link(string? title, Xref xref)
        {
            Title = title;
            Xref = xref;
            IsXref = true;
            Uri = null;
            IsExternal = false;
        }

        public string? Title { get; }

        public string? Uri { get; }

        public Xref? Xref { get; }

        public bool IsXref { get; }

        public bool IsExternal { get; }
    }
}