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
        public static readonly string[] DefaultAssetExtensions = {
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
        };

        /// <summary>
        /// Get asset extensions for a section, falling back to defaults if not specified
        /// </summary>
        /// <param name="sectionDefinition">Section definition that may contain custom asset extensions</param>
        /// <returns>Array of asset extensions to use for the section</returns>
        public static string[] GetAssetExtensions(SectionDefinition? sectionDefinition = null)
        {
            return sectionDefinition?.AssetExtensions ?? DefaultAssetExtensions;
        }

        /// <summary>
        /// Determine if a file path represents an asset based on its extension
        /// </summary>
        /// <param name="filePath">File path to check</param>
        /// <param name="sectionDefinition">Optional section definition for custom extensions</param>
        /// <returns>True if the file should be treated as an asset</returns>
        public static bool IsAsset(string filePath, SectionDefinition? sectionDefinition = null)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return GetAssetExtensions(sectionDefinition).Contains(extension);
        }
    }
}