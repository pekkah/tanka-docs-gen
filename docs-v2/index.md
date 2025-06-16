---
title: Tanka Docs - Documentation Generator
---

# Tanka Docs

Tanka Docs is a powerful technical documentation generator designed for .NET projects, inspired by the Antora project. It transforms your Markdown files and code examples into beautiful, versioned documentation websites.

## 🚀 Key Features

- **Versioned Documentation**: Generate documentation from Git repositories with support for versioning using tags and branches
- **Modular Structure**: Organize documentation using sections for better maintainability
- **Live Code Integration**: Include C# code snippets/files using `#include` syntax with Roslyn integration
- **Cross-References**: Link between documents using `xref://` syntax for maintainable internal links
- **Customizable UI**: Use Handlebars templates for flexible UI customization
- **Git Integration**: Built-in support for Git repositories and version management
- **Static Site Generation**: Generates static HTML sites that can be hosted anywhere

## 📦 Installation

Install Tanka Docs as a .NET global tool:

```bash
dotnet tool install --global Tanka.DocsGen
```

## ⚡ Quick Start

1. **Create configuration file** - Add a `tanka-docs.yml` file to your repository root
2. **Set up documentation sections** - Create folders for your docs with `tanka-docs-section.yml` files
3. **Write your documentation** - Create Markdown files in your sections
4. **Navigation** - Add navigation files
5. **Generate your site** - Run the `tanka-docs` command

```bash
# Generate documentation
tanka-docs build

# Development mode (work in progress)
tanka-docs dev -f tanka-docs-wip.yml
```

## 📚 Documentation Sections

This documentation is organized into the following sections:

### [📁 Project Structure](xref://structure:configuration.md)
Learn how to organize your documentation project, configure `tanka-docs.yml` and `tanka-docs-section.yml` files, and understand the file system structure.

### [✏️ Syntax & Features](xref://syntax:include.md)
Master the Markdown syntax extensions including `#include` directives for code embedding and `xref://` cross-references for internal linking.

### [🏷️ Versioning](xref://versioning:tags-and-branches.md)
Understand how to work with Git branches and tags to create versioned documentation that grows with your project.

### [🎨 UI Customization](xref://ui:ui-bundle.md)
Learn how to customize the appearance and layout of your documentation using Handlebars templates and UI bundles.

## 🛠️ Command Line Interface

Tanka Docs provides two main commands:

### `tanka-docs build`
Generates static documentation from your source files.

**Options:**
- `-f, --file <FILE>` - Specify path to `tanka-docs.yml` configuration file
- `-o, --output <OUTPUT>` - Set custom output directory
- `-b, --build <BUILD>` - Set custom build directory  
- `--base <BASE>` - Set base href for generated HTML pages

### `tanka-docs dev`
Development server mode (coming soon) for live preview during documentation writing.

## 🔧 Configuration Files

Tanka Docs uses two types of configuration files:

- **`tanka-docs.yml`** - Main site configuration (title, output settings, Git sources)
- **`tanka-docs-section.yml`** - Section-specific configuration (ID, title, navigation)

## 💡 Example Project Structure

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

## 🤝 Getting Help

- **Documentation Issues**: Check the troubleshooting guides in each section
- **Feature Requests**: Visit the project repository
- **Questions**: Refer to the detailed guides in each documentation section

---

**Ready to get started?** Begin with the [Project Structure](xref://structure:configuration.md) guide to set up your first documentation project.