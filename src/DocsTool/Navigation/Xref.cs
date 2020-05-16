namespace Tanka.DocsTool.Navigation
{
    public readonly struct Xref
    {
        public Xref(string? sectionId, string path)
        {
            SectionId = sectionId;
            Path = path;
        }


        public string? SectionId { get; }

        public string Path { get; }

    }
}