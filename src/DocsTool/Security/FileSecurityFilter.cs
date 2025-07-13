using System.Text.RegularExpressions;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Security;

/// <summary>
/// Provides security filtering for files to prevent exposure of sensitive files
/// </summary>
public class FileSecurityFilter
{
    private readonly List<Regex> _excludePatterns;
    private readonly HashSet<string> _excludeExtensions;
    private readonly HashSet<string> _excludeDirectories;

    public FileSecurityFilter()
    {
        // Default exclusion patterns - commonly sensitive files
        var defaultPatterns = new[]
        {
            @"^\.env.*$",              // Environment files (.env, .env.local, etc.)
            @"^\.git.*$",              // Git files and directories  
            @".*\.key$",               // Private key files
            @".*\.pem$",               // Certificate files
            @".*\.p12$",               // Certificate files
            @".*\.pfx$",               // Certificate files
            @".*\.crt$",               // Certificate files
            @".*\.cer$",               // Certificate files
            @".*\.keystore$",          // Java keystores
            @".*\.jks$",               // Java keystores
            @"^secrets?\..*$",         // Files starting with secret/secrets
            @"^password.*$",           // Files starting with password
            @"^credentials?\..*$",     // Files starting with credential/credentials
            @"^config\..*$",           // Config files (often contain sensitive data)
            @"^settings\..*$",         // Settings files
            @".*\.backup$",            // Backup files (may contain sensitive data)
            @".*\.bak$",               // Backup files
            @".*\.old$",               // Old backup files
            @".*\.orig$",              // Original backup files
            @".*\.swp$",               // Vim swap files
            @".*\.tmp$",               // Temporary files
            @".*\.temp$",              // Temporary files
            @".*\.log$",               // Log files (may contain sensitive data)
            @"^\.DS_Store$",           // macOS metadata files
            @"^Thumbs\.db$",           // Windows metadata files
            @"^desktop\.ini$",         // Windows metadata files
            @"^\.vscode/.*$",          // VS Code settings
            @"^\.idea/.*$",            // IntelliJ settings
            @"^node_modules/.*$",      // Node.js dependencies
            @"^packages/.*$",          // Package directories
            @"^bin/.*$",               // Binary directories
            @"^obj/.*$",               // .NET object directories
            @"^target/.*$",            // Maven/Gradle target directories
            @"^build/.*$",             // Build output directories
            @"^dist/.*$",              // Distribution directories
            @"^out/.*$",               // Output directories
            @"^\.nuget/.*$",           // NuGet package cache
            @"^\.gradle/.*$",          // Gradle cache
            @"^\.maven/.*$",           // Maven cache
        };

        _excludePatterns = defaultPatterns.Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToList();

        // Default excluded file extensions
        _excludeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".key", ".pem", ".p12", ".pfx", ".crt", ".cer", ".keystore", ".jks",
            ".backup", ".bak", ".old", ".orig", ".swp", ".tmp", ".temp",
            ".exe", ".dll", ".so", ".dylib", ".bin", ".dat",
            ".zip", ".tar", ".gz", ".7z", ".rar"
        };

        // Default excluded directories
        _excludeDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", ".svn", ".hg", ".bzr",
            "node_modules", "packages", "vendor",
            "bin", "obj", "target", "build", "dist", "out",
            ".vscode", ".idea", ".vs",
            ".nuget", ".gradle", ".maven"
        };
    }

    /// <summary>
    /// Determines if a file should be excluded from the files section
    /// </summary>
    /// <param name="file">The file to check</param>
    /// <returns>True if the file should be excluded, false otherwise</returns>
    public bool ShouldExclude(IReadOnlyFile file)
    {
        var fileName = file.Path.GetFileName();
        var extension = file.Path.GetExtension();
        var relativePath = GetRelativePath(file.Path);

        // Check if filename matches any exclusion pattern
        foreach (var pattern in _excludePatterns)
        {
            if (pattern.IsMatch(fileName) || pattern.IsMatch(relativePath))
            {
                return true;
            }
        }

        // Check if extension is in exclusion list
        if (!string.IsNullOrEmpty(extension) && _excludeExtensions.Contains(extension))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if a directory should be excluded from the files section
    /// </summary>
    /// <param name="directory">The directory to check</param>
    /// <returns>True if the directory should be excluded, false otherwise</returns>
    public bool ShouldExclude(IReadOnlyDirectory directory)
    {
        var dirName = directory.Path.GetFileName();
        var relativePath = GetRelativePath(directory.Path);

        // Check if directory name is in exclusion list
        if (_excludeDirectories.Contains(dirName))
        {
            return true;
        }

        // Check if directory path matches any exclusion pattern
        foreach (var pattern in _excludePatterns)
        {
            if (pattern.IsMatch(dirName) || pattern.IsMatch(relativePath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds a custom exclusion pattern
    /// </summary>
    /// <param name="pattern">Regex pattern to exclude</param>
    public void AddExclusionPattern(string pattern)
    {
        try
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _excludePatterns.Add(regex);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {pattern}", nameof(pattern), ex);
        }
    }

    /// <summary>
    /// Adds a custom file extension to exclude
    /// </summary>
    /// <param name="extension">File extension to exclude (with or without leading dot)</param>
    public void AddExcludedExtension(string extension)
    {
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }
        _excludeExtensions.Add(extension);
    }

    /// <summary>
    /// Adds a custom directory name to exclude
    /// </summary>
    /// <param name="directoryName">Directory name to exclude</param>
    public void AddExcludedDirectory(string directoryName)
    {
        _excludeDirectories.Add(directoryName);
    }

    private static string GetRelativePath(FileSystemPath path)
    {
        // Convert absolute path to relative path for pattern matching
        var pathStr = path.ToString();
        if (pathStr.StartsWith("/") || pathStr.Length > 1 && pathStr[1] == ':')
        {
            // For absolute paths, we want to match against the relative portion
            var segments = path.Segments().ToArray();
            return string.Join("/", segments);
        }
        return pathStr;
    }
}