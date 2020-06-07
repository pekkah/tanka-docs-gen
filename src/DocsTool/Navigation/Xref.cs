namespace Tanka.DocsTool.Navigation
{
    public readonly struct Xref
    {
        public Xref(string? version, string? sectionId, string path)
        {
            Version = version;
            SectionId = sectionId;
            Path = path;
        }
        
        public string? SectionId { get; }

        public string Path { get; }

        public string? Version { get; }

        public override string ToString()
        {
            if (Version != null && SectionId != null)
                return $"xref://{SectionId}@{Version}:{Path}";

            if (SectionId != null)
                return $"xref://{SectionId}:{Path}";

            return $"xref://{Path}";
        }
    }
}