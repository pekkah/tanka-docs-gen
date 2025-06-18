using System.IO.Compression;

namespace Tanka.DocsTool.Init;

/// <summary>
/// Utilities for extracting the embedded UI bundle zip archive.
/// </summary>
public static class ZipExtractor
{
    /// <summary>
    /// Extracts the embedded UI bundle to the specified directory.
    /// </summary>
    /// <param name="targetDirectory">Directory to extract the UI bundle to</param>
    /// <param name="overwrite">Whether to overwrite existing files</param>
    /// <exception cref="ArgumentException">Thrown when target directory is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when extraction fails</exception>
    public static void ExtractUiBundleToDirectory(string targetDirectory, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory))
            throw new ArgumentException("Target directory cannot be null or empty", nameof(targetDirectory));
            
        try
        {
            using var zipStream = EmbeddedResources.GetUiBundleZip();
            ExtractZipToDirectory(zipStream, targetDirectory, overwrite);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new InvalidOperationException($"Failed to extract UI bundle to '{targetDirectory}': {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Extracts a zip stream to the specified directory with detailed control.
    /// </summary>
    /// <param name="zipStream">The zip archive stream</param>
    /// <param name="targetDirectory">Directory to extract to</param>
    /// <param name="overwrite">Whether to overwrite existing files</param>
    public static void ExtractZipToDirectory(Stream zipStream, string targetDirectory, bool overwrite = false)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        
        foreach (var entry in archive.Entries)
        {
            var destinationPath = Path.Combine(targetDirectory, entry.FullName);
            
            // Skip directory entries
            if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
                continue;
                
            // Create directory if needed
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Check if file exists and handle overwrite
            if (File.Exists(destinationPath) && !overwrite)
            {
                continue; // Skip existing file
            }
            
            // Extract file
            entry.ExtractToFile(destinationPath, overwrite: overwrite);
        }
    }
    
    /// <summary>
    /// Gets information about the contents of the embedded UI bundle.
    /// </summary>
    /// <returns>List of file paths in the UI bundle</returns>
    public static List<string> GetUiBundleContents()
    {
        using var zipStream = EmbeddedResources.GetUiBundleZip();
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        
        return archive.Entries
            .Where(entry => !entry.FullName.EndsWith('/') && !entry.FullName.EndsWith('\\'))
            .Select(entry => entry.FullName)
            .ToList();
    }
    
    /// <summary>
    /// Checks if the target directory already contains UI bundle files.
    /// </summary>
    /// <param name="targetDirectory">Directory to check</param>
    /// <returns>True if UI bundle files exist in the directory</returns>
    public static bool DirectoryContainsUiBundle(string targetDirectory)
    {
        if (!Directory.Exists(targetDirectory))
            return false;
            
        // Check for key UI bundle files
        var keyFiles = new[] { "article.hbs", "tanka-docs-section.yml" };
        
        return keyFiles.Any(file => File.Exists(Path.Combine(targetDirectory, file)));
    }
} 