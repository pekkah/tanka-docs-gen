using System.Text.RegularExpressions;

namespace Tanka.DocsTool.Init;

/// <summary>
/// Processes configuration templates by replacing placeholder variables.
/// </summary>
public static class TemplateProcessor
{
    /// <summary>
    /// Processes a configuration template by replacing variables with actual values.
    /// </summary>
    /// <param name="template">The template content</param>
    /// <param name="variables">Dictionary of variable names to values</param>
    /// <returns>Processed template with variables replaced</returns>
    public static string ProcessTemplate(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
            return template;
            
        var result = template;
        
        foreach (var variable in variables)
        {
            var placeholder = $"{{{{{variable.Key}}}}}";
            result = result.Replace(placeholder, variable.Value);
        }
        
        return result;
    }
    
    /// <summary>
    /// Creates variable dictionary for template processing based on provided values.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="defaultBranch">Default Git branch name</param>
    /// <param name="outputPath">Custom output path (optional)</param>
    /// <returns>Dictionary of template variables</returns>
    public static Dictionary<string, string> CreateVariables(
        string projectName, 
        string defaultBranch, 
        string? outputPath = null)
    {
        var variables = new Dictionary<string, string>
        {
            ["PROJECT_NAME"] = projectName,
            ["DEFAULT_BRANCH"] = defaultBranch
        };
        
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            variables["OUTPUT_PATH"] = outputPath;
        }
        
        return variables;
    }
    
    /// <summary>
    /// Processes the default production configuration template.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="defaultBranch">Default Git branch name</param>
    /// <param name="outputPath">Custom output path (optional)</param>
    /// <returns>Processed configuration content</returns>
    public static string ProcessDefaultConfig(string projectName, string defaultBranch, string? outputPath = null)
    {
        var template = EmbeddedResources.GetDefaultConfig();
        var variables = CreateVariables(projectName, defaultBranch, outputPath);
        return ProcessTemplate(template, variables);
    }
    
    /// <summary>
    /// Processes the default WIP configuration template.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="outputPath">Custom output path (optional)</param>
    /// <returns>Processed WIP configuration content</returns>
    public static string ProcessDefaultWipConfig(string projectName, string? outputPath = null)
    {
        var template = EmbeddedResources.GetDefaultWipConfig();
        var variables = CreateVariables(projectName, "HEAD", outputPath); // WIP always uses HEAD
        return ProcessTemplate(template, variables);
    }
    
    /// <summary>
    /// Derives a project name from the current directory name.
    /// </summary>
    /// <returns>Sanitized project name derived from directory</returns>
    public static string DeriveProjectNameFromDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dirName = Path.GetFileName(currentDir);
        
        if (string.IsNullOrWhiteSpace(dirName))
            return "Documentation Project";
            
        // Sanitize the directory name for use as a project name
        return SanitizeProjectName(dirName);
    }
    
    /// <summary>
    /// Sanitizes a project name to be suitable for documentation titles.
    /// </summary>
    /// <param name="name">Raw project name</param>
    /// <returns>Sanitized project name</returns>
    public static string SanitizeProjectName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Documentation Project";
            
        // Replace common separators with spaces and title case
        var sanitized = name
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace(".", " ");
            
        // Title case each word
        var words = sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCased = words.Select(word => 
            char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());
            
        return string.Join(" ", titleCased);
    }
    
    /// <summary>
    /// Finds all placeholder variables in a template.
    /// </summary>
    /// <param name="template">Template content to analyze</param>
    /// <returns>List of variable names found in the template</returns>
    public static List<string> FindVariablesInTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            return new List<string>();
            
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);
        var matches = regex.Matches(template);
        
        return matches
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();
    }
    
    /// <summary>
    /// Validates that all required variables are provided for a template.
    /// </summary>
    /// <param name="template">Template content</param>
    /// <param name="variables">Available variables</param>
    /// <returns>List of missing variable names</returns>
    public static List<string> ValidateTemplate(string template, Dictionary<string, string> variables)
    {
        var requiredVariables = FindVariablesInTemplate(template);
        var providedVariables = variables.Keys.ToHashSet();
        
        return requiredVariables
            .Where(variable => !providedVariables.Contains(variable))
            .ToList();
    }
} 