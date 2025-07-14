using System.IO.Compression;
using System.Reflection;

namespace Tanka.DocsTool.Init;

/// <summary>
/// Provides access to embedded resources including the UI bundle and configuration templates.
/// </summary>
public static class EmbeddedResources
{
    private const string ResourcePrefix = "Tanka.DocsTool.Resources.";

    /// <summary>
    /// Gets the embedded UI bundle as a zip archive stream.
    /// </summary>
    /// <returns>Stream containing the ui-bundle.zip data</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource is not found</exception>
    public static Stream GetUiBundleZip()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{ResourcePrefix}ui-bundle.zip";

        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. " +
                                                "Ensure the UI bundle was properly embedded during build.");

        return stream;
    }

    /// <summary>
    /// Gets the default production configuration template.
    /// </summary>
    /// <returns>The tanka-docs.yml template content</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource is not found</exception>
    public static string GetDefaultConfig()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{ResourcePrefix}default-tanka-docs.yml";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. " +
                                                "Ensure the configuration template was properly embedded during build.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Gets the default WIP/development configuration template.
    /// </summary>
    /// <returns>The tanka-docs-wip.yml template content</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource is not found</exception>
    public static string GetDefaultWipConfig()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{ResourcePrefix}default-tanka-docs-wip.yml";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. " +
                                                "Ensure the WIP configuration template was properly embedded during build.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Checks if the UI bundle resource exists.
    /// </summary>
    /// <returns>True if the UI bundle is embedded, false otherwise</returns>
    public static bool HasUiBundleZip()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{ResourcePrefix}ui-bundle.zip";
        return assembly.GetManifestResourceNames().Contains(resourceName);
    }

    /// <summary>
    /// Lists all embedded resource names (useful for debugging).
    /// </summary>
    /// <returns>Array of embedded resource names</returns>
    public static string[] GetEmbeddedResourceNames()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceNames();
    }
}