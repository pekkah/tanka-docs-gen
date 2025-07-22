using System.IO;
using System.Linq;
using Tanka.DocsTool.Definitions;

namespace Tanka.DocsTool.UI
{
    /// <summary>
    /// Unified configuration for asset file types and extensions
    /// </summary>
    public static class AssetConfiguration
    {
        /// <summary>
        /// Default asset file extensions supported by the system
        /// </summary>
        public static readonly IReadOnlySet<string> DefaultAssetExtensions = new HashSet<string>(new[] {
            // Web assets  
            ".js", ".css", ".woff", ".woff2", ".ttf", ".eot",
            
            // Images
            ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp", ".bmp", ".ico", ".tiff",
            
            // Documents
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt",
            
            // Archives
            ".zip", ".tar", ".gz", ".7z", ".rar",
            
            // Media
            ".mp4", ".webm", ".mov", ".avi", ".mp3", ".wav", ".ogg",
            
            // Data
            ".json", ".xml", ".csv", ".yaml", ".yml"
        }, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Get asset extensions for a section, falling back to defaults if not specified
        /// </summary>
        /// <param name="sectionDefinition">Section definition that may contain custom asset extensions</param>
        /// <returns>Collection of asset extensions to use for the section</returns>
        public static IEnumerable<string> GetAssetExtensions(SectionDefinition? sectionDefinition = null)
        {
            return (IEnumerable<string>)sectionDefinition?.AssetExtensions ?? DefaultAssetExtensions;
        }

        /// <summary>
        /// Determine if a file path represents an asset based on its extension
        /// </summary>
        /// <param name="filePath">File path to check</param>
        /// <param name="sectionDefinition">Optional section definition for custom extensions</param>
        /// <returns>True if the file should be treated as an asset</returns>
        public static bool IsAsset(string filePath, SectionDefinition? sectionDefinition = null)
        {
            var extension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension)) return false;

            var extensions = GetAssetExtensions(sectionDefinition);
            
            // For section-specific extensions (string[]), use case-insensitive comparison
            if (sectionDefinition?.AssetExtensions != null)
            {
                return extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            }
            
            // For default extensions (HashSet with OrdinalIgnoreCase), use direct Contains
            return DefaultAssetExtensions.Contains(extension);
        }
    }
}