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

        public Xref WithVersion(string version) => new Xref(version, SectionId, Path);

        public Xref WithSectionId(string sectionId) => new Xref(Version, sectionId, Path);

        public Xref WithPath(string path) => new Xref(Version, SectionId, path);
    }
}