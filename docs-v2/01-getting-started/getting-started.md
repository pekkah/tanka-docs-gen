---
title: Quick Start Guide
---

# Quick Start Guide

This guide will walk you through setting up your first Tanka Docs project from scratch. By the end, you'll have a working documentation site that you can build and preview locally.

## Prerequisites

- **.NET 9** installed on your system
- **Git** (Tanka Docs sources content from Git repositories)
- **Text editor** or IDE of your choice

## Step 1: Install Tanka Docs

Install Tanka Docs as a .NET global tool:

```bash
# Install from NuGet (recommended)
dotnet tool install --global Tanka.DocsGen
```

**Alternative installation from source:**

```bash
# Clone and build from source (for development)
git clone https://github.com/pekkah/tanka-docs-gen.git
cd tanka-docs-gen
dotnet pack -c Release -o ./artifacts
dotnet tool install --global --add-source ./artifacts Tanka.DocsGen
```

## Step 2: Create Your Documentation Project

Create a new directory for your documentation project and initialize it:

```bash
mkdir my-docs-project
cd my-docs-project
git init -b main
```

## Step 3: Initialize Tanka Docs

Use the `init` command to set up your project automatically:

```bash
tanka-docs init
```

This command creates:

- **`tanka-docs.yml`** - Production configuration file with sensible defaults
- **`tanka-docs-wip.yml`** - Development configuration for work-in-progress builds
- **`ui-bundle/`** - Customizable UI templates and styling

**Initialization Options:**

```bash
# Initialize with custom project name
tanka-docs init --project-name "My Documentation Site"

# Initialize with specific branch
tanka-docs init --branch main

# Only create configuration files (skip UI bundle)
tanka-docs init --config-only

# Force overwrite existing files
tanka-docs init --force

# Quiet mode (skip guidance output)
tanka-docs init --quiet
```

**Generated Configuration:**

The init command creates a `tanka-docs.yml` with the following structure:

```yaml
name: "My Docs Project"
description: "Documentation for My Docs Project"

# Content sources - Git branches/tags for stable builds
sources:
  - source: git-branch
    branch: "main"
    path: docs/

# Output configuration
output_path: _build/
build_path: _site/

# UI configuration
ui_bundle: ui-bundle/

# Processing options
extensions:
  - include
  - xref

# Development settings (override in tanka-docs-wip.yml for WIP builds)
base_path: ""
```

## Step 4: Create Documentation Content

Create your documentation directory and start writing content:

```bash
mkdir docs
```

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
- [Advanced Topics](xref://advanced:index.md) - Deep dive into advanced features

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
  * [Advanced Topics](xref://advanced:index.md)
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
- [Learn about advanced features](xref://advanced:index.md)
```

Create additional pages (`docs/configuration.md`, `docs/advanced.md`) following the same pattern.

## Step 5: Commit Your Content

Tanka Docs sources content from Git, so commit your changes:

```bash
git add .
git commit -m "Initial documentation setup"
```

## Step 6: Build Your Documentation

Build your documentation site:

```bash
tanka-docs build
```

If successful, you'll see output showing the build progress and generated files in the `_build/` directory.

## Step 7: Preview Your Documentation

Use the development server for live preview:

```bash
tanka-docs dev
```

This will:
- Build your documentation
- Start a web server (default port 8080)
- Watch for file changes and rebuild automatically
- Provide live reload in the browser

Open your browser and navigate to `http://localhost:8080` to view your documentation.

For custom configuration, you can specify options:

```bash
# Use WIP configuration for development
tanka-docs dev -f tanka-docs-wip.yml

# Custom port
tanka-docs dev --port 3000

# Custom output directory
tanka-docs build -o ./dist
```

## Step 8: Add a `.gitignore`

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
\#include::xref://src:example.cs
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