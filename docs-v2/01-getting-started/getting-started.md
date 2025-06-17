---
title: Quick Start Guide
---

# Quick Start Guide

This guide will walk you through setting up your first Tanka Docs project from scratch. By the end, you'll have a working documentation site that you can build and preview locally.

## Prerequisites

- **.NET 9** installed on your system
- **Git** (Tanka Docs sources content from Git repositories)
- **Text editor** or IDE of your choice

## Step 1: Clone the Project

Tanka Docs is currently distributed as source code. Clone the repository:

```bash
git clone https://github.com/pekkah/tanka-docs-gen.git
cd tanka-docs-gen
```

## Step 2: Build the Tool

Build the Tanka Docs tool:

```bash
dotnet build
```

## Step 3: Create Your Documentation Project

Create a new directory for your documentation project:

```bash
mkdir my-docs-project
cd my-docs-project
git init -b main
```

## Step 4: Create the Main Configuration File

Create `tanka-docs.yml` in your project root. This file defines your site structure and content sources:

```yaml
base_path: "/"
title: "My Documentation Site"
index_page: xref://docs@HEAD:index.md
output_path: "gh-pages"
build_path: "_build"
branches:
  HEAD:
    input_path:
      - docs
      - _partials
```

**Key Configuration Options:**

- `base_path`: The base URL path for your site (usually "/")
- `title`: Your site's title
- `index_page`: The main landing page using xref syntax
- `output_path`: Where the built site will be generated
- `build_path`: Temporary build directory
- `branches`: Git branches/refs to source content from
- `input_path`: Directories to include from each branch

## Step 5: Create Documentation Sections

Tanka Docs organizes content into sections. Create your main documentation section:

```bash
mkdir docs
```

Create `docs/tanka-docs-section.yml`:

```yaml
id: "docs"
title: "Documentation"
index_page: "xref://index.md"
nav:
  - xref://nav.md
includes:
  - "**/*.md"
```

**Section Configuration:**

- `id`: Unique identifier for the section
- `title`: Display title for the section
- `index_page`: Default page for this section
- `nav`: Navigation file(s) for this section
- `includes`: File patterns to include in this section

## Step 6: Create Content Files

### Main Index Page

Create `docs/index.md`:

```markdown
---
title: Welcome to My Documentation
---

# Welcome to My Documentation

This is the home page of your documentation site. 

## Quick Navigation

- [Installation](xref://installation.md) - Get up and running quickly
- [Configuration](xref://configuration.md) - Learn how to configure your project
- [Advanced Topics](xref://advanced.md) - Deep dive into advanced features

## Getting Started

Follow our [installation guide](xref://installation.md) to begin using the system.

## Need Help?

Check out our documentation sections for detailed information on specific topics.
```

### Navigation File

Create `docs/nav.md`:

```markdown
* Documentation
  * [Welcome](xref://index.md)
  * [Installation](xref://installation.md)
  * [Configuration](xref://configuration.md)
  * [Advanced Topics](xref://advanced.md)
```

### Content Pages

Create `docs/installation.md`:

```markdown
---
title: Installation
---

# Installation

This guide covers installing and setting up the system.

## System Requirements

- .NET 9 or later
- Git
- 4GB RAM minimum

## Installation Steps

1. Download the latest release
2. Extract to your desired location
3. Run the setup script

## Verification

Verify the installation:

```bash
your-command --version
```

## Next Steps

- [Configure your installation](xref://configuration.md)
- [Learn about advanced features](xref://advanced.md)
```

Create additional pages (`docs/configuration.md`, `docs/advanced.md`) following the same pattern.

## Step 7: Add Shared Content (Optional)

Create a partials section for shared content:

```bash
mkdir _partials
```

Create `_partials/tanka-docs-section.yml`:

```yaml
id: "partials"
title: "Shared Content"
includes:
  - "**/*.md"
```

Create `_partials/common-notice.md`:

```markdown
> **Note:** This is shared content that can be included in multiple pages using the include syntax.
```

## Step 8: Commit Your Content

Tanka Docs sources content from Git, so commit your changes:

```bash
git add .
git commit -m "Initial documentation setup"
```

## Step 9: Build Your Documentation

Build your documentation site using the Tanka Docs tool:

```bash
# From the tanka-docs-gen project directory
dotnet run --project src/DocsTool -- build -f /path/to/your/project/tanka-docs.yml
```

If successful, you'll see output similar to:

```
Tanka Docs
Initializing...
CurrentPath: /path/to/your/project
ConfigFilePath: /path/to/your/project/tanka-docs.yml
OutputPath: gh-pages
BuildPath: _build
```

## Step 10: Preview Your Documentation

Use the development server for live preview:

```bash
# From the tanka-docs-gen project directory
dotnet run --project src/DocsTool -- dev --port 8080 -f /path/to/your/project/tanka-docs.yml
```

This will:
- Build your documentation
- Start a web server on the specified port
- Watch for file changes and rebuild automatically
- Provide live reload in the browser

Open your browser and navigate to `http://localhost:8080` to view your documentation.

## Step 11: Add a `.gitignore`

Create a `.gitignore` file in your project:

```gitignore
# Tanka Docs build artifacts
_build/
gh-pages/

# OS files
.DS_Store
Thumbs.db
```

## Advanced Features

### Including Source Code

Include code snippets from your source files:

```markdown
#include::src/example.cs
```

### Cross-References

Link between pages and sections:

```markdown
- [Configuration Guide](xref://configuration.md)
- [Advanced Section](xref://advanced-section:some-page.md)
```

### Versioning

Configure multiple versions using Git branches and tags:

```yaml
branches:
  main:
    input_path: [docs]
  develop:
    input_path: [docs]
tags:
  "v1.*":
    input_path: [docs]
  "v2.*":
    input_path: [docs]
```

## Troubleshooting

### Common Issues

**Build fails with "Configuration not found":**
- Ensure `tanka-docs.yml` exists in your project root
- Check YAML syntax is valid

**Pages not appearing:**
- Verify `nav.md` references match actual file names
- Check that `tanka-docs-section.yml` includes patterns match your files
- Ensure content is committed to Git

**Cross-references not working:**
- Use the correct xref syntax: `xref://section:file.md` or `xref://file.md`
- Verify target files exist in the referenced sections

### Getting Detailed Information

Use the `--debug` flag for verbose output:

```bash
dotnet run --project src/DocsTool -- build --debug -f tanka-docs.yml
```

### Getting Help

- Review the [Configuration Reference](xref://project-structure:tanka-docs-yml.md)
- Check the [File System Structure](xref://project-structure:file-system.md) guide
- Examine the [Tanka Docs repository](https://github.com/pekkah/tanka-docs-gen) for examples

## What's Next?

Now that you have a basic documentation site:

1. **Add more content** - Create additional pages and sections
2. **Customize the UI** - Create a custom UI bundle with Handlebars templates
3. **Set up versioning** - Configure multiple Git branches/tags for different versions
4. **Automate builds** - Set up CI/CD to automatically build and deploy your documentation
5. **Learn advanced syntax** - Use includes, cross-references, and other advanced features 