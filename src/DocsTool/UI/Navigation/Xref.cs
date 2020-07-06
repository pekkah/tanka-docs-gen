using System;
using System.Collections.Generic;

namespace Tanka.DocsTool.Navigation
{
    public readonly struct Xref: IEquatable<Xref>
    {
        public Xref(string? version, string? sectionId, string path, IReadOnlyDictionary<string, string>? query = null)
        {
            Version = version;
            SectionId = sectionId;
            Path = path;
            Query = query ?? new Dictionary<string, string>();
        }

        public IReadOnlyDictionary<string, string> Query { get; }

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

        public bool Equals(Xref other)
        {
            return SectionId == other.SectionId && Path == other.Path && Version == other.Version;
        }

        public override bool Equals(object? obj)
        {
            return obj is Xref other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SectionId, Path, Version);
        }
    }
}