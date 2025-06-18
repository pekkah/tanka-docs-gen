---
title: Tanka Docs - Documentation Generator
---

# Tanka Docs

Tanka Docs is a powerful technical documentation generator designed for .NET projects, inspired by the Antora project. It transforms your Markdown files and code examples into beautiful, versioned documentation websites.

## Key Features

- **Versioned Documentation**: Generate documentation from Git repositories with support for versioning using tags and branches
- **Modular Structure**: Organize documentation using sections for better maintainability
- **Live Code Integration**: Include C# code snippets/files using `#include` syntax with Roslyn integration
- **Cross-References**: Link between documents using `xref://` syntax for maintainable internal links
- **Customizable UI**: Use Handlebars templates for flexible UI customization
- **Git Integration**: Built-in support for Git repositories and version management
- **Static Site Generation**: Generates static HTML sites that can be hosted anywhere

## Installation

Install Tanka Docs as a .NET global tool:

```bash
dotnet tool install --global Tanka.DocsGen
```

## Quick Start

1. **Initialize your project** - Run `tanka-docs init` in your Git repository
2. **Create documentation content** - Add Markdown files to the `docs/` folder
3. **Build your site** - Use `tanka-docs build` or `tanka-docs dev`

```bash
# Initialize a new project (creates config and UI templates)
tanka-docs init

# Create your content
mkdir docs
echo "# Welcome" > docs/index.md

# Generate documentation
tanka-docs build

# Run in development mode with live preview
tanka-docs dev
```

## Documentation Sections

This documentation is organized into the following sections:

### [Getting Started](xref://getting-started:index.md)
Quick start guide and basic configuration to get up and running with Tanka Docs.

### [Project Structure](xref://project-structure:index.md)
Learn how to organize your documentation project, configure `tanka-docs.yml` and `tanka-docs-section.yml` files, and understand the file system structure.

### [Writing Content](xref://writing-content:index.md)
Master the Markdown syntax extensions including `#include` directives for code embedding and `xref://` cross-references for internal linking.

### [Advanced Features](xref://advanced:index.md)
Explore advanced features including Git-based versioning, CLI reference, and troubleshooting guides.

### [UI Customization](xref://customization:index.md)
Learn how to customize the appearance and layout of your documentation using Handlebars templates and UI bundles.

## Command Line Interface

Tanka Docs provides three main commands:

### `tanka-docs init`
Initialize a new documentation project with default configuration and UI templates.

**Options:**
- `--project-name <NAME>` - Set custom project name
- `--branch <BRANCH>` - Specify default Git branch
- `--config-only` - Only create configuration files
- `--force` - Overwrite existing files

### `tanka-docs build`
Generates static documentation from your source files.

**Options:**
- `-f, --file <FILE>` - Specify path to `tanka-docs.yml` configuration file
- `-o, --output <o>` - Set custom output directory
- `-b, --build <BUILD>` - Set custom build directory  
- `--base <BASE>` - Set base href for generated HTML pages

### `tanka-docs dev`
Starts a local development server with live preview and hot-reloading.

## Configuration Files

Tanka Docs uses two types of configuration files:

- **`tanka-docs.yml`** - Main site configuration (title, output settings, Git sources)
- **`tanka-docs-section.yml`** - Section-specific configuration (ID, title, navigation)

## Example Project Structure

```
my-project/
├── tanka-docs.yml              # Main configuration
├── docs/
│   ├── tanka-docs-section.yml  # Section configuration
│   ├── index.md                # Section home page
│   ├── getting-started.md
│   └── advanced/
│       └── configuration.md
├── api-docs/
│   ├── tanka-docs-section.yml
│   └── reference.md
└── src/
    └── MyProject/
        └── Program.cs          # Can be included in docs
```

## Getting Help

- **Documentation Issues**: Check the troubleshooting guides in each section
- **Feature Requests**: Visit the project repository
- **Questions**: Refer to the detailed guides in each documentation section

---

**Ready to get started?** Begin with the [Getting Started](xref://getting-started:index.md) guide to set up your first documentation project.