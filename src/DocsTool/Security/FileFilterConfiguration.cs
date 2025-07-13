namespace Tanka.DocsTool.Security;

/// <summary>
/// Configuration for file filtering in files sections
/// </summary>
public class FileFilterConfiguration
{
    /// <summary>
    /// Whether to enable security filtering (default: true)
    /// </summary>
    public bool EnableSecurityFiltering { get; set; } = true;

    /// <summary>
    /// Additional file patterns to exclude (regex patterns)
    /// </summary>
    public List<string> ExcludePatterns { get; set; } = new();

    /// <summary>
    /// Additional file extensions to exclude (with or without leading dot)
    /// </summary>
    public List<string> ExcludeExtensions { get; set; } = new();

    /// <summary>
    /// Additional directory names to exclude
    /// </summary>
    public List<string> ExcludeDirectories { get; set; } = new();

    /// <summary>
    /// File patterns to explicitly include (overrides exclusions)
    /// </summary>
    public List<string> IncludePatterns { get; set; } = new();

    /// <summary>
    /// Directories to explicitly include (overrides exclusions)  
    /// </summary>
    public List<string> IncludeDirectories { get; set; } = new();

    /// <summary>
    /// Whether to be case sensitive in pattern matching (default: false)
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    // TODO: File size filtering would require async API changes
    // /// <summary>
    // /// Maximum file size to include (in bytes, default: 10MB)
    // /// </summary>
    // public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Whether to include hidden files (files starting with .) (default: false)
    /// </summary>
    public bool IncludeHiddenFiles { get; set; } = false;
}