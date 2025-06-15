# Tanka Documentation Generator

[![master](https://dev.azure.com/tanka-ops/docs-gen/_apis/build/status/docs-gen?branchName=master)](https://dev.azure.com/tanka-ops/docs-gen/_build/latest?definitionId=2&branchName=master)

Tanka Docs is a powerful technical documentation generator designed for .NET projects, inspired by the Antora project. It transforms your Markdown files and code examples into beautiful, versioned documentation websites.

## 🚀 Key Features

- **Versioned Documentation**: Generate documentation from Git repositories with support for versioning using tags and branches
- **Modular Structure**: Organize documentation using sections for better maintainability  
- **Live Code Integration**: Include C# code snippets/files using `#include` syntax with Roslyn integration
- **Cross-References**: Link between documents using `xref://` syntax for maintainable internal links
- **Customizable UI**: Use Handlebars templates for flexible UI customization
- **Git Integration**: Built-in support for Git repositories and version management
- **Static Site Generation**: Generates static HTML sites that can be hosted anywhere

## 📦 Quick Start

### 1. Install

```bash
dotnet tool install --global Tanka.DocsGen
```

### 2. Create Configuration

Add `tanka-docs.yml` to your project root:

```yaml
title: "My Documentation Site"
description: "Documentation for my project"

output_path: "gh-pages"
build_path: "_build"

sources:
  - name: "local"
    type: "local" 
    path: "."
```

### 3. Set Up Documentation Section

Create `docs/tanka-docs-section.yml`:

```yaml
id: "docs"
title: "Documentation"
type: "docs"
index_page: "index.md"

navigation:
  - title: "Getting Started"
    items:
      - title: "Introduction"
        page: "index.md"
```

### 4. Write Documentation

Create `docs/index.md`:

```markdown
---
title: Welcome
---

# Welcome to My Documentation

This is your documentation homepage.

## Features

- Easy to use
- Version controlled
- Beautiful output

## Code Examples

\```csharp
#include::xref://src:Program.cs?s=Main
\```
```

### 5. Build

```bash
tanka-docs build
```

### 6. Serve Locally

```bash
dotnet tool install --global dotnet-serve
cd gh-pages
dotnet serve
```

## 📚 Documentation

Comprehensive documentation is available at: **[https://pekkah.github.io/tanka-docs-gen](https://pekkah.github.io/tanka-docs-gen)**

### Quick Links

- **[Getting Started Guide](https://pekkah.github.io/tanka-docs-gen/structure/getting-started.html)** - Step-by-step setup instructions
- **[CLI Reference](https://pekkah.github.io/tanka-docs-gen/structure/cli-reference.html)** - Command line options and usage
- **[Include Directives](https://pekkah.github.io/tanka-docs-gen/syntax/include.html)** - Embedding code and content
- **[Cross-References](https://pekkah.github.io/tanka-docs-gen/syntax/xref.html)** - Internal linking system
- **[Troubleshooting](https://pekkah.github.io/tanka-docs-gen/structure/troubleshooting.html)** - Common issues and solutions

## 🛠️ Command Line Usage

### Build Documentation

```bash
# Basic build
tanka-docs build

# With custom configuration
tanka-docs build -f custom-config.yml

# With custom output directory  
tanka-docs build -o ./dist

# With debug output
tanka-docs build --debug

# For subdirectory deployment
tanka-docs build --base "/my-docs/"
```

### Development Mode

```bash
# Development server (work in progress)
tanka-docs dev
```

## 📁 Project Structure

```
my-project/
├── tanka-docs.yml              # Main configuration
├── docs/                       # Documentation section
│   ├── tanka-docs-section.yml  # Section configuration
│   ├── index.md               # Documentation files
│   └── getting-started.md
├── _partials/                  # Shared content
│   ├── tanka-docs-section.yml
│   └── common-notice.md
└── src/                        # Source code (for includes)
    └── Program.cs
```

## ✨ Advanced Features

### Include Code Snippets

Include entire files or specific symbols:

```markdown
\```csharp
# Include entire file
#include::xref://src:Program.cs

# Include specific method
#include::xref://src:MyClass.cs?s=MyNamespace.MyClass.MyMethod

# Include from another section
#include::xref://examples:sample.cs
\```
```

### Cross-Reference Links

Create maintainable internal links:

```markdown
[Getting Started](xref://getting-started.md)
[API Reference](xref://api:overview.md)
[Version 1.0 Docs](xref://docs@1.0.0:index.md)
```

### Versioning with Git

```yaml
# tanka-docs.yml
sources:
  - name: "main"
    type: "git"
    url: "https://github.com/user/repo.git"
    branches: ["main", "develop"]
    tags: ["v*"]
```

## 🎨 Customization

### Custom UI Bundle

Create custom templates using Handlebars:

```html
<!-- ui-bundle/layouts/default.hbs -->
<!DOCTYPE html>
<html>
<head>
    <title>{{page.title}} | {{site.title}}</title>
</head>
<body>
    <header>{{site.title}}</header>
    <main>{{{content}}}</main>
</body>
</html>
```

### Custom Styling

```css
/* ui-bundle/css/custom.css */
:root {
  --primary-color: #007acc;
  --background-color: #f8f9fa;
}
```

## 🔧 Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `title` | Site title | - |
| `description` | Site description | - |
| `output_path` | Output directory | `gh-pages` |
| `build_path` | Build cache directory | `_build` |
| `base_path` | Base URL path | `/` |
| `ui_bundle` | UI template bundle | `default` |

## 📋 Requirements

- **.NET 6+** - Required for running the tool
- **Git** - Optional, for version control and Git sources
- **Text Editor** - Any editor that supports Markdown

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: [https://pekkah.github.io/tanka-docs-gen](https://pekkah.github.io/tanka-docs-gen)
- **Issues**: [GitHub Issues](https://github.com/pekkah/tanka-docs-gen/issues)
- **Discussions**: [GitHub Discussions](https://github.com/pekkah/tanka-docs-gen/discussions)

## 📈 Roadmap

- [ ] Development server with live reload
- [ ] Plugin system for extensions
- [ ] Additional source types (GitHub, GitLab)
- [ ] Enhanced theme customization
- [ ] Performance optimizations
- [ ] Multi-language support
