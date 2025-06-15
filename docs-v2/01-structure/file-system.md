---
title: File System Structure
---

# File System Structure

Understanding how Tanka Docs organizes and processes files is essential for creating well-structured documentation projects. This guide explains the file system conventions, directory structures, and file types used by Tanka Docs.

## Project Root Structure

A typical Tanka Docs project follows this structure:

```
my-project/
├── tanka-docs.yml              # Main site configuration
├── docs/                       # Documentation section
│   ├── tanka-docs-section.yml  # Section configuration
│   ├── index.md               # Section index page
│   ├── getting-started.md     # Documentation files
│   └── advanced/              # Subdirectories for organization
│       └── configuration.md
├── api-reference/             # Another documentation section
│   ├── tanka-docs-section.yml
│   ├── index.md
│   └── classes/
│       └── myclass.md
├── _partials/                 # Shared content snippets
│   ├── tanka-docs-section.yml
│   └── common-notice.md
├── ui-bundle/                 # Custom UI templates (optional)
│   ├── layouts/
│   └── partials/
└── src/                       # Source code (for includes)
    └── MyProject/
        └── Program.cs
```

## Configuration Files

### `tanka-docs.yml` (Site Configuration)

This is the main configuration file that must be placed at the root of your project. It defines:

- Site metadata (title, description)
- Content sources (Git repositories, branches, tags)
- Output settings (build directory, output directory)
- UI bundle configuration

**Location:** Project root
**Format:** YAML
**Required:** Yes

### `tanka-docs-section.yml` (Section Configuration)

Each documentation section requires this configuration file in its root directory. It defines:

- Section identity (ID, title, type)
- Navigation structure
- Index page specification
- Section-specific settings

**Location:** Root of each section directory
**Format:** YAML
**Required:** Yes (for each section)

## Content Organization

### Sections

Sections are the primary organizational unit in Tanka Docs. Each section:

- Has its own directory with a `tanka-docs-section.yml` file
- Contains related documentation on a specific topic
- Can have its own navigation structure
- Supports versioning independently

**Common section types:**
- `docs` - General documentation
- `partials` - Shared content snippets (prefixed with `_`)
- `api` - API reference documentation

### File Types

#### Markdown Files (`.md`)

- **Purpose:** Documentation content
- **Processing:** Converted to HTML with Tanka Docs extensions
- **Features:** Support for `\#include::` directives and `xref://` links
- **Naming:** Use kebab-case (e.g., `getting-started.md`)

#### Include Sources

Any text-based file can be included in documentation:

- **Code files:** `.cs`, `.js`, `.py`, `.yaml`, etc.
- **Markdown snippets:** Reusable content blocks
- **Configuration examples:** Sample configuration files
- **Text files:** Plain text content

## Directory Naming Conventions

### Section Directories

- Use descriptive names that reflect the content
- Avoid spaces; use hyphens for separation
- Consider future organization and growth

**Good examples:**
- `getting-started/`
- `api-reference/`
- `user-guide/`
- `_partials/` (for shared content)

**Avoid:**
- `docs/` (too generic if you have multiple sections)
- `section1/` (non-descriptive)
- `my docs/` (spaces)

### File Organization

Within sections, organize files logically:

```
user-guide/
├── tanka-docs-section.yml
├── index.md                    # Section home
├── installation.md             # Top-level topics
├── configuration.md
├── advanced/                   # Grouped topics
│   ├── custom-templates.md
│   ├── performance.md
│   └── security.md
└── examples/                   # Supporting materials
    ├── basic-config.yml
    └── advanced-config.yml
```

## Path Resolution

### Relative Paths

Tanka Docs resolves paths relative to the configuration file location:

- **Working directory:** Directory containing `tanka-docs.yml`
- **Section paths:** Relative to the working directory
- **Include paths:** Resolved using `xref://` syntax
- **Asset paths:** Relative to the section directory

### Cross-References

Use `xref://` syntax for maintainable internal links:

```markdown
[Link to same section](xref://other-page.md)
[Link to different section](xref://section-id:page.md)
[Link to specific version](xref://section-id@1.0.0:page.md)
```

## Special Directories

### Build Directory

- **Purpose:** Temporary files during generation
- **Default:** `_build/`
- **Contents:** Processed content before final output
- **Cleanup:** Can be safely deleted

### Output Directory

- **Purpose:** Final generated static site
- **Default:** `gh-pages/` or configured in `tanka-docs.yml`
- **Contents:** HTML, CSS, JS, and assets
- **Deployment:** This directory is what you deploy

### Cache Directory

- **Purpose:** Caches downloaded Git content and processed files
- **Location:** Within build directory
- **Benefit:** Faster subsequent builds
- **Cleanup:** Safe to delete to force fresh build

## File Processing Pipeline

1. **Discovery:** Scan for configuration files and sections
2. **Content Loading:** Read Markdown and source files
3. **Preprocessing:** Process `\#include::` directives
4. **Cross-Reference Resolution:** Process `xref://` links
5. **Markdown Processing:** Convert to HTML
6. **Template Application:** Apply UI bundle templates
7. **Asset Copying:** Copy static assets
8. **Output Generation:** Write final files to output directory

## Best Practices

### Organization

- Group related content in subdirectories
- Use consistent naming conventions
- Keep section directories focused and cohesive
- Separate shared content into `_partials` sections

### File Naming

- Use lowercase with hyphens (kebab-case)
- Make names descriptive and searchable
- Avoid special characters and spaces
- Use consistent file extensions (`.md` for Markdown)

### Path Management

- Use `xref://` for all internal links
- Keep include paths relative to section roots
- Organize includes logically (by type or feature)
- Document your file organization in a README

### Performance

- Avoid deeply nested directory structures
- Keep individual files reasonably sized
- Use includes to break up large documents
- Clean build directories periodically

## Troubleshooting

### Common Issues

**Files not found during build:**
- Check file paths are correct
- Verify `xref://` syntax
- Ensure files exist at specified locations

**Includes not working:**
- Verify include syntax: `\#include::xref://...`
- Check that referenced files exist
- Use `--debug` flag to see detailed processing

**Navigation problems:**
- Check `tanka-docs-section.yml` navigation configuration
- Verify file references in navigation match actual files
- Ensure proper YAML syntax in configuration files
