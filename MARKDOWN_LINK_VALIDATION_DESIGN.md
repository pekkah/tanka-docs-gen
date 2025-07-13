# Markdown Link Validation System Design

## Overview

This document outlines the design for a build-time link validation system that works with standard Markdown links, providing similar guarantees to the XREF system while maintaining compatibility with standard Markdown tools.

## Core Components

### 1. Link Extractor

Extracts all links from Markdown files during build process.

```csharp
public interface IMarkdownLinkExtractor
{
    IEnumerable<MarkdownLink> ExtractLinks(string content, string filePath);
}

public class MarkdownLink
{
    public string Text { get; set; }
    public string Target { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public LinkType Type { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public enum LinkType
{
    Relative,      // ./file.md, ../dir/file.md
    Absolute,      // /docs/file.md
    External,      // https://example.com
    Anchor,        // #section
    Email,         // mailto:user@example.com
    Reference      // [link][ref]
}
```

### 2. Link Validator

Validates each extracted link based on type and context.

```csharp
public interface ILinkValidator
{
    ValidationResult ValidateLink(MarkdownLink link, ValidationContext context);
}

public class ValidationContext
{
    public string SourceFile { get; set; }
    public IFileSystem FileSystem { get; set; }
    public DocumentationSite Site { get; set; }
    public ValidationOptions Options { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string Error { get; set; }
    public string Suggestion { get; set; }
    public string ResolvedPath { get; set; }
}
```

### 3. Smart Link Resolver

Resolves links with intelligent fallback mechanisms.

```csharp
public class SmartLinkResolver : ILinkResolver
{
    public ResolvedLink Resolve(string link, LinkContext context)
    {
        // Try exact match first
        var exact = TryExactMatch(link, context);
        if (exact != null) return exact;

        // Try with different extensions
        var extensions = new[] { ".md", ".html", "/index.md", "/index.html" };
        foreach (var ext in extensions)
        {
            var result = TryWithExtension(link, ext, context);
            if (result != null) return result;
        }

        // Try case-insensitive match
        var caseInsensitive = TryCaseInsensitive(link, context);
        if (caseInsensitive != null) return caseInsensitive;

        // Try fuzzy match for moved files
        var fuzzy = TryFuzzyMatch(link, context);
        if (fuzzy != null) 
        {
            return fuzzy with 
            { 
                Warning = $"Link target may have moved to: {fuzzy.Path}" 
            };
        }

        return new ResolvedLink { IsValid = false, Error = "Target not found" };
    }
}
```

### 4. Link Metadata Parser

Parses optional metadata comments for enhanced features.

```csharp
public class LinkMetadataParser
{
    // Parses comments like:
    // <!-- @link-id: unique-id -->
    // <!-- @version: 2.0+ -->
    // <!-- @section: api -->
    public LinkMetadata ParseMetadata(string content, int linkLine)
    {
        var metadata = new LinkMetadata();
        var lines = content.Split('\n');
        
        // Look for metadata comments after the link
        for (int i = linkLine; i < Math.Min(linkLine + 3, lines.Length); i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("<!-- @"))
            {
                ParseMetadataLine(line, metadata);
            }
        }
        
        return metadata;
    }
}
```

### 5. Validation Report Generator

Generates comprehensive validation reports.

```csharp
public class ValidationReportGenerator
{
    public void GenerateReport(IEnumerable<ValidationIssue> issues, ReportOptions options)
    {
        if (options.Format == ReportFormat.Console)
        {
            foreach (var issue in issues)
            {
                Console.WriteLine($"{issue.Severity}: {issue.File}:{issue.Line}");
                Console.WriteLine($"  {issue.Message}");
                if (!string.IsNullOrEmpty(issue.Suggestion))
                {
                    Console.WriteLine($"  Suggestion: {issue.Suggestion}");
                }
            }
        }
        else if (options.Format == ReportFormat.Json)
        {
            var json = JsonSerializer.Serialize(issues);
            File.WriteAllText(options.OutputPath, json);
        }
        else if (options.Format == ReportFormat.Sarif)
        {
            // Generate SARIF format for IDE integration
            GenerateSarifReport(issues, options);
        }
    }
}
```

## Validation Rules

### 1. Internal Link Validation

```csharp
public class InternalLinkValidator : ILinkValidator
{
    public ValidationResult ValidateLink(MarkdownLink link, ValidationContext context)
    {
        // Resolve the link path
        var resolved = ResolveInternalPath(link.Target, context.SourceFile);
        
        // Check if file exists
        if (!context.FileSystem.FileExists(resolved))
        {
            // Try smart resolution
            var smartResult = SmartResolve(link.Target, context);
            if (smartResult.Found)
            {
                return new ValidationResult
                {
                    IsValid = true,
                    Warning = $"Found via smart resolution: {smartResult.Path}",
                    ResolvedPath = smartResult.Path
                };
            }
            
            return new ValidationResult
            {
                IsValid = false,
                Error = $"File not found: {resolved}",
                Suggestion = GetSimilarFiles(resolved, context)
            };
        }
        
        // Check anchor if present
        if (link.Target.Contains('#'))
        {
            var anchor = link.Target.Substring(link.Target.IndexOf('#') + 1);
            if (!ValidateAnchor(resolved, anchor, context))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Error = $"Anchor not found: #{anchor}"
                };
            }
        }
        
        return new ValidationResult { IsValid = true, ResolvedPath = resolved };
    }
}
```

### 2. External Link Validation (Optional)

```csharp
public class ExternalLinkValidator : ILinkValidator
{
    private readonly HttpClient _httpClient;
    
    public async Task<ValidationResult> ValidateLinkAsync(MarkdownLink link, ValidationContext context)
    {
        if (!context.Options.ValidateExternalLinks)
            return new ValidationResult { IsValid = true };
            
        try
        {
            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, link.Target));
                
            if (response.IsSuccessStatusCode)
            {
                return new ValidationResult { IsValid = true };
            }
            
            return new ValidationResult
            {
                IsValid = false,
                Error = $"External link returned {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                Error = $"Failed to check external link: {ex.Message}"
            };
        }
    }
}
```

### 3. Version-Aware Validation

```csharp
public class VersionAwareLinkValidator : ILinkValidator
{
    public ValidationResult ValidateLink(MarkdownLink link, ValidationContext context)
    {
        // Extract version from link or metadata
        var version = ExtractVersion(link, context);
        
        // Check if target exists in specified version
        var versionedPath = GetVersionedPath(link.Target, version);
        
        if (!context.FileSystem.FileExists(versionedPath))
        {
            // Try to find in other versions
            var availableVersions = FindAvailableVersions(link.Target, context);
            
            return new ValidationResult
            {
                IsValid = false,
                Error = $"File not found in version {version}",
                Suggestion = $"Available in versions: {string.Join(", ", availableVersions)}"
            };
        }
        
        return new ValidationResult { IsValid = true, ResolvedPath = versionedPath };
    }
}
```

## Build Integration

### 1. MSBuild Task

```xml
<Project>
  <UsingTask TaskName="ValidateMarkdownLinks" 
             AssemblyFile="$(TankaDocsTasksPath)" />
  
  <Target Name="ValidateDocumentationLinks" BeforeTargets="Build">
    <ValidateMarkdownLinks
      SourceDirectory="$(DocsDirectory)"
      ConfigFile="link-validation.yml"
      TreatWarningsAsErrors="$(TreatLinkWarningsAsErrors)"
      GenerateReport="true"
      ReportPath="$(ArtifactsDirectory)/link-validation.json" />
  </Target>
</Project>
```

### 2. Configuration File

```yaml
# link-validation.yml
validation:
  internal_links: true
  external_links: false  # Optional, can be slow
  anchors: true
  case_sensitive: false
  
resolution:
  smart_resolution: true
  try_extensions: [.md, .html, /index.md, /index.html]
  fuzzy_matching: true
  
rules:
  - pattern: "*.api.md"
    version_aware: true
    section: api
    
  - pattern: "examples/**/*.md"
    allow_missing: true  # Examples might reference future features
    
exclusions:
  - "drafts/**/*"
  - "**/*.template.md"
  
reporting:
  format: console  # console, json, sarif
  verbosity: normal  # quiet, normal, detailed
  exit_code_on_error: true
```

### 3. CLI Integration

```bash
# Validate all links
tanka-docs validate-links

# Validate with specific config
tanka-docs validate-links -c custom-validation.yml

# Fix common issues automatically
tanka-docs validate-links --fix

# Generate detailed report
tanka-docs validate-links --report-format sarif -o validation.sarif
```

## Auto-Fix Capabilities

```csharp
public class LinkAutoFixer
{
    public IEnumerable<LinkFix> SuggestFixes(ValidationIssue issue)
    {
        var fixes = new List<LinkFix>();
        
        // Fix wrong extension
        if (issue.Type == IssueType.WrongExtension)
        {
            fixes.Add(new LinkFix
            {
                Description = "Change extension to .md",
                Replacement = ChangeExtension(issue.Link, ".md")
            });
        }
        
        // Fix wrong case
        if (issue.Type == IssueType.WrongCase)
        {
            fixes.Add(new LinkFix
            {
                Description = "Fix casing",
                Replacement = GetCorrectCasing(issue.Link)
            });
        }
        
        // Fix moved file
        if (issue.Type == IssueType.FileMoved)
        {
            fixes.Add(new LinkFix
            {
                Description = $"Update to new location",
                Replacement = GetNewLocation(issue.Link)
            });
        }
        
        return fixes;
    }
    
    public void ApplyFixes(IEnumerable<LinkFix> fixes, bool interactive = true)
    {
        foreach (var fix in fixes)
        {
            if (interactive)
            {
                Console.WriteLine($"Apply fix: {fix.Description}?");
                Console.WriteLine($"  Old: {fix.OldValue}");
                Console.WriteLine($"  New: {fix.Replacement}");
                
                if (Console.ReadKey().Key != ConsoleKey.Y)
                    continue;
            }
            
            ApplyFix(fix);
        }
    }
}
```

## Performance Optimizations

### 1. Parallel Processing

```csharp
public class ParallelLinkValidator
{
    public async Task<ValidationReport> ValidateAsync(IEnumerable<string> files)
    {
        var results = await files
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .SelectAsync(async file => await ValidateFileAsync(file))
            .ToListAsync();
            
        return new ValidationReport(results);
    }
}
```

### 2. Caching

```csharp
public class CachedLinkValidator
{
    private readonly IMemoryCache _cache;
    
    public ValidationResult ValidateLink(MarkdownLink link, ValidationContext context)
    {
        var cacheKey = $"{context.SourceFile}:{link.Target}";
        
        if (_cache.TryGetValue<ValidationResult>(cacheKey, out var cached))
        {
            return cached;
        }
        
        var result = PerformValidation(link, context);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }
}
```

## Benefits Achieved

1. **Build-time Validation** ✅
   - All links validated during build
   - Configurable validation rules
   - Clear error reporting

2. **Smart Resolution** ✅
   - Handles missing extensions
   - Case-insensitive matching
   - Fuzzy matching for moved files

3. **Version Awareness** ✅
   - Via path conventions or metadata
   - Cross-version link validation

4. **IDE Integration** ✅
   - SARIF output for VS Code
   - Real-time validation possible

5. **Backward Compatibility** ✅
   - Works with standard Markdown
   - No special syntax required

6. **Performance** ✅
   - Parallel processing
   - Caching for large projects
   - Incremental validation

This design provides a robust link validation system that works with standard Markdown while offering many of the benefits of the XREF system.