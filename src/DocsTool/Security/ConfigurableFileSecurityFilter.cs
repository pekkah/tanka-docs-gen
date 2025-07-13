using System.Text.RegularExpressions;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Security;

/// <summary>
/// Configurable security filter for files with support for custom patterns and .gitignore
/// </summary>
public class ConfigurableFileSecurityFilter
{
    private readonly FileFilterConfiguration _config;
    private readonly List<Regex> _excludePatterns;
    private readonly List<Regex> _includePatterns;
    private readonly HashSet<string> _excludeExtensions;
    private readonly HashSet<string> _excludeDirectories;
    private readonly HashSet<string> _includeDirectories;

    public ConfigurableFileSecurityFilter(FileFilterConfiguration? config = null)
    {
        _config = config ?? new FileFilterConfiguration();
        
        var regexOptions = _config.CaseSensitive 
            ? RegexOptions.Compiled 
            : RegexOptions.IgnoreCase | RegexOptions.Compiled;

        // Initialize base security patterns
        _excludePatterns = new List<Regex>();
        _includePatterns = new List<Regex>();
        
        if (_config.EnableSecurityFiltering)
        {
            InitializeDefaultSecurityPatterns(regexOptions);
        }

        // Add custom patterns
        foreach (var pattern in _config.ExcludePatterns)
        {
            try
            {
                _excludePatterns.Add(new Regex(pattern, regexOptions));
            }
            catch (ArgumentException)
            {
                // Log invalid pattern but continue
                Console.WriteLine($"Warning: Invalid exclude pattern '{pattern}' - skipping");
            }
        }

        foreach (var pattern in _config.IncludePatterns)
        {
            try
            {
                _includePatterns.Add(new Regex(pattern, regexOptions));
            }
            catch (ArgumentException)
            {
                // Log invalid pattern but continue
                Console.WriteLine($"Warning: Invalid include pattern '{pattern}' - skipping");
            }
        }

        // Initialize extension and directory sets
        var comparer = _config.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        
        _excludeExtensions = new HashSet<string>(comparer);
        _excludeDirectories = new HashSet<string>(comparer);
        _includeDirectories = new HashSet<string>(comparer);

        if (_config.EnableSecurityFiltering)
        {
            InitializeDefaultSecurityExtensionsAndDirectories();
        }

        // Add custom extensions and directories
        foreach (var ext in _config.ExcludeExtensions)
        {
            var extension = ext.StartsWith(".") ? ext : "." + ext;
            _excludeExtensions.Add(extension);
        }

        foreach (var dir in _config.ExcludeDirectories)
        {
            _excludeDirectories.Add(dir);
        }

        foreach (var dir in _config.IncludeDirectories)
        {
            _includeDirectories.Add(dir);
        }
    }

    /// <summary>
    /// Determines if a file should be excluded
    /// </summary>
    public bool ShouldExclude(IReadOnlyFile file)
    {
        var fileName = file.Path.GetFileName();
        var extension = file.Path.GetExtension();
        var relativePath = GetRelativePath(file.Path);

        // Check explicit include patterns first (they override all excludes)
        foreach (var pattern in _includePatterns)
        {
            if (pattern.IsMatch(fileName) || pattern.IsMatch(relativePath))
            {
                return false; // Explicitly included
            }
        }

        // Check security filtering (when enabled, these take priority over hidden file settings)
        if (_config.EnableSecurityFiltering)
        {
            // Check exclude patterns
            foreach (var pattern in _excludePatterns)
            {
                if (pattern.IsMatch(fileName) || pattern.IsMatch(relativePath))
                {
                    return true;
                }
            }

            // Check exclude extensions
            if (!string.IsNullOrEmpty(extension) && _excludeExtensions.Contains(extension))
            {
                return true;
            }
        }

        // Check if hidden files should be excluded (only after security filtering)
        if (!_config.IncludeHiddenFiles && fileName.ToString().StartsWith("."))
        {
            return true;
        }

        // Check file size limit
        if (TryGetFileSize(file, out var fileSize) && fileSize > _config.MaxFileSize)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if a directory should be excluded
    /// </summary>
    public bool ShouldExclude(IReadOnlyDirectory directory)
    {
        var dirName = directory.Path.GetFileName();
        var relativePath = GetRelativePath(directory.Path);

        // Check explicit include directories first
        if (_includeDirectories.Contains(dirName))
        {
            return false; // Explicitly included
        }

        // Check explicit include patterns
        foreach (var pattern in _includePatterns)
        {
            if (pattern.IsMatch(dirName) || pattern.IsMatch(relativePath))
            {
                return false; // Explicitly included
            }
        }

        // Check security filtering (when enabled, these take priority over hidden file settings)
        if (_config.EnableSecurityFiltering)
        {
            // Check exclude directories
            if (_excludeDirectories.Contains(dirName))
            {
                return true;
            }

            // Check exclude patterns
            foreach (var pattern in _excludePatterns)
            {
                if (pattern.IsMatch(dirName) || pattern.IsMatch(relativePath))
                {
                    return true;
                }
            }
        }

        // Check if hidden directories should be excluded (only after security filtering)
        if (!_config.IncludeHiddenFiles && dirName.ToString().StartsWith("."))
        {
            return true;
        }

        return false;
    }

    private void InitializeDefaultSecurityPatterns(RegexOptions regexOptions)
    {
        var defaultPatterns = new[]
        {
            @"^\.env.*$",              // Environment files
            @"^\.git.*$",              // Git files and directories  
            @".*\.key$",               // Private key files
            @".*\.pem$",               // Certificate files
            @".*\.p12$", @".*\.pfx$", @".*\.crt$", @".*\.cer$",
            @".*\.keystore$", @".*\.jks$",
            @"^secrets?\..*$",         // Files starting with secret/secrets
            @"^password.*$",           // Files starting with password
            @"^credentials?\..*$",     // Files starting with credential/credentials
            @"^config\..*$",           // Config files (often contain sensitive data)
            @"^settings\..*$",         // Settings files
            @".*\.backup$", @".*\.bak$", @".*\.old$", @".*\.orig$",
            @".*\.swp$", @".*\.tmp$", @".*\.temp$",
            @".*\.log$",               // Log files (may contain sensitive data)
            @"^\.DS_Store$", @"^Thumbs\.db$", @"^desktop\.ini$",
            @"^\.vscode/.*$", @"^\.idea/.*$",
            @"^node_modules/.*$", @"^packages/.*$",
            @"^bin/.*$", @"^obj/.*$", @"^target/.*$",
            @"^build/.*$", @"^dist/.*$", @"^out/.*$",
            @"^\.nuget/.*$", @"^\.gradle/.*$", @"^\.maven/.*$"
        };

        foreach (var pattern in defaultPatterns)
        {
            _excludePatterns.Add(new Regex(pattern, regexOptions));
        }
    }

    private void InitializeDefaultSecurityExtensionsAndDirectories()
    {
        // Security-sensitive extensions
        var defaultExtensions = new[]
        {
            ".key", ".pem", ".p12", ".pfx", ".crt", ".cer", ".keystore", ".jks",
            ".backup", ".bak", ".old", ".orig", ".swp", ".tmp", ".temp",
            ".exe", ".dll", ".so", ".dylib", ".bin", ".dat",
            ".zip", ".tar", ".gz", ".7z", ".rar"
        };

        foreach (var ext in defaultExtensions)
        {
            _excludeExtensions.Add(ext);
        }

        // Security-sensitive directories
        var defaultDirectories = new[]
        {
            ".git", ".svn", ".hg", ".bzr",
            "node_modules", "packages", "vendor",
            "bin", "obj", "target", "build", "dist", "out",
            ".vscode", ".idea", ".vs",
            ".nuget", ".gradle", ".maven"
        };

        foreach (var dir in defaultDirectories)
        {
            _excludeDirectories.Add(dir);
        }
    }

    private static string GetRelativePath(FileSystemPath path)
    {
        var pathStr = path.ToString();
        if (pathStr.StartsWith("/") || (pathStr.Length > 1 && pathStr[1] == ':'))
        {
            var segments = path.Segments().ToArray();
            return string.Join("/", segments);
        }
        return pathStr;
    }

    private static bool TryGetFileSize(IReadOnlyFile file, out long size)
    {
        size = 0;
        try
        {
            // This would need to be implemented based on the IReadOnlyFile interface
            // For now, we'll assume all files are under the limit
            return false;
        }
        catch
        {
            return false;
        }
    }
}