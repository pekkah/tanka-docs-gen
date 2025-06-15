---
title: Troubleshooting Guide
---

# Troubleshooting Guide

This guide covers common issues you might encounter when using Tanka Docs and their solutions.

## Installation Issues

### Tool Installation Fails

**Error:**
```
Could not execute because the specified command or file was not found.
```

**Solution:**
1. Ensure you have .NET 6+ installed:
   ```bash
   dotnet --version
   ```
2. If .NET is not installed, download from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
3. Retry the installation:
   ```bash
   dotnet tool install --global Tanka.DocsGen
   ```

### Permission Errors During Installation

**Error:**
```
Access to the path is denied.
```

**Solution:**
1. On Windows: Run as Administrator
2. On macOS/Linux: Use `sudo` if necessary, or install to user directory:
   ```bash
   dotnet tool install --global Tanka.DocsGen --tool-path ~/.dotnet/tools
   ```

## Configuration Issues

### Configuration File Not Found

**Error:**
```
Could not load configuration: 'tanka-docs.yml'
```

**Solutions:**
1. **Check file exists:**
   ```bash
   ls -la tanka-docs.yml
   ```

2. **Check current directory:**
   ```bash
   pwd
   ```

3. **Specify configuration path:**
   ```bash
   tanka-docs build -f path/to/tanka-docs.yml
   ```

### Invalid YAML Syntax

**Error:**
```
YamlDotNet.Core.YamlException: Invalid YAML
```

**Solutions:**
1. **Validate YAML syntax** using online YAML validators
2. **Check indentation** - YAML is sensitive to spaces/tabs
3. **Check quotes** - Use consistent quote styles
4. **Common issues:**
   - Mixed tabs and spaces
   - Missing colons after keys
   - Incorrect list formatting

**Example of correct YAML:**
```yaml
title: "My Site"
sources:
  - name: "local"
    type: "local"
    path: "."
```

## Build Issues

### Section Configuration Missing

**Error:**
```
Section configuration not found: tanka-docs-section.yml
```

**Solution:**
1. **Check each section directory** has a `tanka-docs-section.yml` file
2. **Verify file name** is exactly `tanka-docs-section.yml`
3. **Example section configuration:**
   ```yaml
   id: "docs"
   title: "Documentation"
   type: "docs"
   index_page: "index.md"
   ```

### Referenced Files Not Found

**Error:**
```
File not found: installation.md
```

**Solutions:**
1. **Check file exists:**
   ```bash
   ls -la docs/installation.md
   ```

2. **Check navigation references:**
   ```yaml
   navigation:
     - title: "Installation"
       page: "installation.md"  # Must match actual filename
   ```

3. **Check cross-references:**
   ```markdown
   [Link](xref://installation.md)  # Must match actual filename
   ```

### Include Files Not Found

**Error:**
```
Include file not found: src/Program.cs
```

**Solutions:**
 1. **Verify include syntax:**
    ```markdown
    \#include::xref://src:Program.cs
    ```

2. **Check file exists:**
   ```bash
   ls -la src/Program.cs
   ```

3. **Use debug mode** to see detailed processing:
   ```bash
   tanka-docs build --debug
   ```

## Git and Versioning Issues

### Git Repository Issues

**Error:**
```
LibGit2Sharp.RepositoryNotFoundException: Path does not exist
```

**Solutions:**
1. **Initialize Git repository:**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   ```

2. **Check Git status:**
   ```bash
   git status
   ```

3. **For local-only documentation** (no Git), use local source:
   ```yaml
   sources:
     - name: "local"
       type: "local"
       path: "."
   ```

### Version Detection Issues

**Error:**
```
No versions found in repository
```

**Solutions:**
1. **Create at least one tag:**
   ```bash
   git tag v1.0.0
   ```

2. **Check existing tags:**
   ```bash
   git tag -l
   ```

3. **Use branches for versions:**
   ```bash
   git checkout -b docs-v1.0
   ```

## Include Directive Issues

### Roslyn Symbol Not Found

**Error:**
```
Symbol not found: MyNamespace.MyClass.MyMethod
```

**Solutions:**
1. **Check symbol exists** in the specified file
 2. **Use fully qualified name:**
    ```markdown
    \#include::xref://src:MyFile.cs?s=MyNamespace.MyClass.MyMethod
    ```

3. **Check file compiles:**
   ```bash
   dotnet build
   ```

4. **List available symbols** in debug mode:
   ```bash
   tanka-docs build --debug
   ```

### Escaped Includes Not Working

**Problem:** Escaped includes still get processed

**Solution:**
Ensure you're using the correct escape syntax:
```markdown
\#include::xref://src:Program.cs
```

If this doesn't work, check you're using the latest version of Tanka Docs.

## Output and Deployment Issues

### Permission Denied Writing Output

**Error:**
```
Access to the path 'gh-pages' is denied.
```

**Solutions:**
1. **Check directory permissions:**
   ```bash
   ls -la gh-pages/
   ```

2. **Change permissions if needed:**
   ```bash
   chmod -R 755 gh-pages/
   ```

3. **Use different output directory:**
   ```bash
   tanka-docs build -o ./my-output
   ```

### Generated Links Not Working

**Problem:** Links return 404 errors when deployed

**Solutions:**
1. **Set correct base path** for subdirectory deployment:
   ```bash
   tanka-docs build --base "/my-docs/"
   ```

2. **Check server configuration** for static file serving

3. **Verify relative paths** in generated HTML

## Performance Issues

### Slow Build Times

**Solutions:**
1. **Clean build directory:**
   ```bash
   rm -rf _build/
   ```

2. **Reduce processed content:**
   - Remove unused files from sections
   - Use `.gitignore` patterns in configuration

3. **Optimize includes:**
   - Include specific symbols instead of entire files
   - Cache frequently used includes

### High Memory Usage

**Solutions:**
1. **Process smaller batches** by splitting large sections
2. **Remove large binary files** from source directories
3. **Use include directives** instead of copying large files

## Debug Mode

For detailed troubleshooting information, always use debug mode:

```bash
tanka-docs build --debug
```

This provides:
- Detailed processing steps
- File resolution information
- Error stack traces
- Performance timing
- Git repository information

## Common Configuration Patterns

### Working Directory Issues

**Problem:** Relative paths not resolving correctly

**Solution:** Understand working directory behavior:
- Working directory = directory containing `tanka-docs.yml`
- All relative paths resolve from working directory
- Use `-f` option to specify config file location

### Multiple Environment Configurations

**Pattern:** Different configs for dev/prod

**Solution:**
```bash
# Development
tanka-docs build -f config/dev.yml -o ./dev-output

# Production  
tanka-docs build -f config/prod.yml -o ./dist --base "/docs/"
```

## Getting Additional Help

### Enable Detailed Logging

```bash
tanka-docs build --debug > build.log 2>&1
```

### Check Version

```bash
tanka-docs --version
```

### Update to Latest Version

```bash
dotnet tool update --global Tanka.DocsGen
```

### Validate Configuration

Before building, validate your configuration manually:

1. **Check YAML syntax** with online validators
2. **Verify all referenced files exist**
3. **Test with minimal configuration** first
4. **Use relative paths consistently**

## Error Message Index

| Error Pattern | Section |
|---------------|---------|
| `Could not load configuration` | Configuration Issues |
| `File not found` | Build Issues |
| `YamlException` | Configuration Issues |
| `RepositoryNotFoundException` | Git and Versioning Issues |
| `Access denied` | Output and Deployment Issues |
| `Symbol not found` | Include Directive Issues |

## Still Need Help?

If you can't resolve your issue with this guide:

1. **Check the documentation** for the specific feature you're using
2. **Search existing issues** in the project repository
3. **Create a minimal reproduction case**
4. **Include the full error message and debug output**
5. **Specify your environment** (OS, .NET version, Tanka Docs version) 