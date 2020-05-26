using MimeDb;
using Tanka.DocsTool.Catalogs;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Pipelines
{
    public class MimeDbClassifier : IContentClassifier
    {
        public string Classify(IReadOnlyFile file)
        {
            var path = file.Path.ToString();

            var extension = System.IO.Path.GetExtension(path);
            var filename = System.IO.Path.GetFileNameWithoutExtension(path);

            // builtIn overrides
            var builtIn = GetBuiltIn(filename, extension);

            if (!string.IsNullOrEmpty(builtIn))
                return builtIn;

            // mime-db lookup
            if (MimeType.TryGet(extension, out var mimeType)) return mimeType.Type;

            return ContentTypes.Unknown;
        }

        public string? GetBuiltIn(string filename, string extension)
        {
            return (filename, extension) switch
            {
                ("tanka-docs", ".yml") => ContentTypes.SiteDefinition,
                ("tanka-docs-section", ".yml") => ContentTypes.SectionDefinition,
                _ => null
            };
        }
    }
}