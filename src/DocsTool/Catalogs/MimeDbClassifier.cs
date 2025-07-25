﻿using MimeDb;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Catalogs
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

            return "__unknown";
        }

        public string? GetBuiltIn(string filename, string extension)
        {
            return (filename, extension) switch
            {
                { filename: "tanka-docs-section", extension: ".yml" or ".yaml" } => ContentItem.SectionDefinitionType,
                (_, _) => null
            };
        }
    }
}