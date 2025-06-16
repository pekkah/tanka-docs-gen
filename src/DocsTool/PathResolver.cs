using System.IO;

namespace Tanka.DocsTool;

public static class PathResolver
{
    public static (string currentPath, string configFilePath) ResolvePaths(string? configFile)
    {
        var currentPath = Directory.GetCurrentDirectory();
        var configFilePath = Path.Combine(currentPath, "tanka-docs.yml");
        configFilePath = Path.GetFullPath(configFilePath);

        if (!string.IsNullOrEmpty(configFile))
        {
            configFilePath = Path.GetFullPath(configFile);
            currentPath = Path.GetDirectoryName(configFilePath) ?? "";
        }

        return (currentPath, configFilePath);
    }
} 