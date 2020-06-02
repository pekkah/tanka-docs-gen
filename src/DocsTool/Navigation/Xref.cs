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

        public override string ToString()
        {
            if (SectionId != null)
                return $"xref://{SectionId}:{Path}";

            return $"xref://{Path}";
        }
    }
}